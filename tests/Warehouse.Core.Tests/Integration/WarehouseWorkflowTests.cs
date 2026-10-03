using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;
using Warehouse.Core.Security;
using Warehouse.Core.Services;
using Xunit;

namespace Warehouse.Core.Tests.Integration
{
    /// <summary>End-to-end tests of the services against a real SQL Server database.</summary>
    [Collection(LocalDbCollection.Name)]
    public class WarehouseWorkflowTests
    {
        private readonly LocalDbFixture _database;

        public WarehouseWorkflowTests(LocalDbFixture database)
        {
            _database = database;
        }

        [SkippableFact]
        public void Provisioning_creates_a_consistent_demo_database_once()
        {
            RequireDatabase();

            Assert.False(DatabaseProvisioner.EnsureCreated(_database.ConnectionString, seedDemoData: true));

            using (var connection = new SqlConnection(_database.ConnectionString))
            {
                connection.Open();

                // Ledger invariant: stock on hand equals the sum of the product's movements.
                Assert.Equal(0, Scalar(connection, @"
                    SELECT COUNT(*) FROM dbo.Products p
                    WHERE p.QuantityOnHand <> (SELECT ISNULL(SUM(m.Quantity), 0) FROM dbo.StockMovements m WHERE m.ProductId = p.Id)"));

                // Reservation invariant: reserved stock equals the quantities on picked orders.
                Assert.Equal(0, Scalar(connection, @"
                    SELECT COUNT(*) FROM dbo.Products p
                    WHERE p.QuantityReserved <> (SELECT ISNULL(SUM(l.Quantity), 0) FROM dbo.OrderLines l
                                                 JOIN dbo.Orders o ON o.Id = l.OrderId
                                                 WHERE l.ProductId = p.Id AND o.Status = 1)"));
            }

            using (var db = _database.CreateContext())
            {
                Assert.True(db.Products.Any(p => p.Sku == "PKG-1001"));
                Assert.True(new AccountService(db).SignIn("demo", "Demo@12345").Succeeded);
            }
        }

        [SkippableFact]
        public void Receive_pick_and_ship_update_stock_and_the_ledger()
        {
            RequireDatabase();
            var (productId, customerId, carrierId) = CreateMasterData(stock: 50);
            int orderId;

            using (var db = _database.CreateContext())
            {
                var orders = new OrderService(db);
                orderId = orders.Create(customerId, new[] { new OrderLineInput(productId, 20) }, "PO-42", "tester").Id;
                orders.Pick(orderId, "picker");
                orders.Ship(orderId, carrierId, "TRK-42", "shipper");
            }

            using (var db = _database.CreateContext())
            {
                var product = db.Products.Single(p => p.Id == productId);
                Assert.Equal(30, product.QuantityOnHand);
                Assert.Equal(0, product.QuantityReserved);

                var order = new OrderService(db).Find(orderId);
                Assert.Equal(OrderStatus.Shipped, order.Status);
                Assert.Equal(carrierId, order.CarrierId);
                Assert.Equal("TRK-42", order.TrackingNumber);

                var movements = db.StockMovements.Where(m => m.ProductId == productId).OrderBy(m => m.Id).ToList();
                Assert.Equal(new[] { 50, -20 }, movements.Select(m => m.Quantity));
                Assert.Equal(orderId, movements[1].OrderId);
                Assert.Equal(order.Number, movements[1].Reference);
            }
        }

        [SkippableFact]
        public void Shipping_the_same_order_twice_is_rejected()
        {
            RequireDatabase();
            var (productId, customerId, carrierId) = CreateMasterData(stock: 400);

            using (var db = _database.CreateContext())
            {
                var orders = new OrderService(db);
                var orderId = orders.Create(customerId, new[] { new OrderLineInput(productId, 100) }, null, "tester").Id;
                orders.Pick(orderId, "picker");
                orders.Ship(orderId, carrierId, null, "shipper");

                Assert.Throws<BusinessRuleException>(() => orders.Ship(orderId, carrierId, null, "shipper"));
            }

            using (var db = _database.CreateContext())
            {
                Assert.Equal(300, db.Products.Single(p => p.Id == productId).QuantityOnHand);
            }
        }

        [SkippableFact]
        public void Concurrent_shipments_of_the_same_order_are_detected()
        {
            RequireDatabase();
            var (productId, customerId, carrierId) = CreateMasterData(stock: 10);
            int orderId;

            using (var db = _database.CreateContext())
            {
                var orders = new OrderService(db);
                orderId = orders.Create(customerId, new[] { new OrderLineInput(productId, 5) }, null, "tester").Id;
                orders.Pick(orderId, "picker");
            }

            using (var first = _database.CreateContext())
            using (var second = _database.CreateContext())
            {
                // Both users open the order before either of them ships it.
                var staleOrder = new OrderService(second).Find(orderId);
                var carrier = second.Carriers.Find(carrierId);

                new OrderService(first).Ship(orderId, carrierId, null, "first-user");

                second.StockMovements.AddRange(staleOrder.Ship(carrier, null, "second-user", DateTime.UtcNow));
                Assert.Throws<ConcurrencyConflictException>(() => second.Commit());
            }

            using (var db = _database.CreateContext())
            {
                Assert.Equal(5, db.Products.Single(p => p.Id == productId).QuantityOnHand);
                Assert.Equal(1, db.StockMovements.Count(m => m.OrderId == orderId));
            }
        }

        [SkippableFact]
        public void Concurrent_picks_cannot_reserve_more_than_is_available()
        {
            RequireDatabase();
            var (productId, customerId, _) = CreateMasterData(stock: 10);
            int firstOrderId, secondOrderId;

            using (var db = _database.CreateContext())
            {
                var orders = new OrderService(db);
                firstOrderId = orders.Create(customerId, new[] { new OrderLineInput(productId, 8) }, null, "tester").Id;
                secondOrderId = orders.Create(customerId, new[] { new OrderLineInput(productId, 8) }, null, "tester").Id;
            }

            using (var first = _database.CreateContext())
            using (var second = _database.CreateContext())
            {
                var firstOrder = new OrderService(first).Find(firstOrderId);
                var secondOrder = new OrderService(second).Find(secondOrderId);

                firstOrder.Pick("picker-1", DateTime.UtcNow);
                secondOrder.Pick("picker-2", DateTime.UtcNow); // each context still sees 10 available

                first.Commit();
                Assert.Throws<ConcurrencyConflictException>(() => second.Commit());
            }

            using (var db = _database.CreateContext())
            {
                Assert.Equal(8, db.Products.Single(p => p.Id == productId).QuantityReserved);
            }
        }

        [SkippableFact]
        public void Picking_queue_flags_orders_that_are_short_of_stock()
        {
            RequireDatabase();
            var (productId, customerId, _) = CreateMasterData(stock: 5);

            using (var db = _database.CreateContext())
            {
                var orders = new OrderService(db);
                var ready = orders.Create(customerId, new[] { new OrderLineInput(productId, 5) }, null, "tester").Id;
                var shortOrder = orders.Create(customerId, new[] { new OrderLineInput(productId, 6) }, null, "tester").Id;

                var queue = orders.QueryPickingQueue().Where(o => o.Id == ready || o.Id == shortOrder).ToDictionary(o => o.Id);

                Assert.True(queue[ready].IsReady);
                Assert.False(queue[shortOrder].IsReady);
                Assert.Throws<BusinessRuleException>(() => orders.Pick(shortOrder, "picker"));
            }
        }

        [SkippableFact]
        public void Editing_a_product_with_a_stale_version_is_rejected()
        {
            RequireDatabase();
            var (productId, _, _) = CreateMasterData(stock: 0);
            byte[] versionSeenByUser;

            using (var db = _database.CreateContext())
            {
                versionSeenByUser = db.Products.AsNoTracking().Single(p => p.Id == productId).RowVersion;
            }

            using (var db = _database.CreateContext())
            {
                new InventoryService(db).Receive(productId, 3, "DN-1", "receiver");
            }

            using (var db = _database.CreateContext())
            {
                var details = new ProductDetails { Sku = "STALE-" + productId, Name = "Renamed" };
                Assert.Throws<ConcurrencyConflictException>(() => new ProductService(db).Update(productId, details, versionSeenByUser));
            }
        }

        [SkippableFact]
        public void Records_with_history_cannot_be_deleted()
        {
            RequireDatabase();
            var (productId, customerId, _) = CreateMasterData(stock: 10);

            using (var db = _database.CreateContext())
            {
                new OrderService(db).Create(customerId, new[] { new OrderLineInput(productId, 1) }, null, "tester");

                Assert.Throws<BusinessRuleException>(() => new ProductService(db).Delete(productId));
                Assert.Throws<BusinessRuleException>(() => new CustomerService(db).Delete(customerId));
            }
        }

        [SkippableFact]
        public void Duplicate_skus_are_rejected()
        {
            RequireDatabase();

            using (var db = _database.CreateContext())
            {
                var ex = Assert.Throws<BusinessRuleException>(() =>
                    new ProductService(db).Create(new ProductDetails { Sku = "pkg-1001", Name = "Duplicate" }));

                Assert.Contains("PKG-1001", ex.Message);
            }
        }

        [SkippableFact]
        public void Accounts_lock_after_repeated_failed_sign_ins()
        {
            RequireDatabase();
            var userName = "user" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            using (var db = _database.CreateContext())
            {
                var accounts = new AccountService(db, new PasswordHasher(iterations: 1_000), () => now);
                accounts.Register(userName, "S3cure-pass");
                Assert.Throws<BusinessRuleException>(() => accounts.Register(userName.ToUpperInvariant(), "S3cure-pass"));

                Assert.True(accounts.SignIn(userName, "S3cure-pass").Succeeded);

                SignInResult result = null;
                for (var attempt = 0; attempt < User.MaxFailedSignIns; attempt++)
                {
                    result = accounts.SignIn(userName, "wrong-password");
                }

                Assert.True(result.IsLockedOut);
                Assert.True(accounts.SignIn(userName, "S3cure-pass").IsLockedOut);

                now = now.Add(User.LockoutDuration);
                Assert.True(accounts.SignIn(userName, "S3cure-pass").Succeeded);
            }
        }

        private (int ProductId, int CustomerId, int CarrierId) CreateMasterData(int stock)
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();

            using (var db = _database.CreateContext())
            {
                var product = new ProductService(db).Create(new ProductDetails { Sku = "TEST-" + suffix, Name = "Test product " + suffix, ReorderLevel = 1 });
                if (stock > 0)
                {
                    new InventoryService(db).Receive(product.Id, stock, "DN-" + suffix, "receiver");
                }

                var customer = new CustomerService(db).Create(new PartnerDetails { Name = "Customer " + suffix });
                var carrier = new CarrierService(db).Create(new PartnerDetails { Name = "Carrier " + suffix });
                return (product.Id, customer.Id, carrier.Id);
            }
        }

        private void RequireDatabase() => Skip.IfNot(_database.IsAvailable, _database.UnavailableReason);

        private static int Scalar(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                return (int)command.ExecuteScalar();
            }
        }
    }
}
