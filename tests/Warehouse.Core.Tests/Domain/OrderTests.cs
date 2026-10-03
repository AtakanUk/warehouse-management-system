using System.Linq;
using Warehouse.Core.Domain;
using Xunit;

namespace Warehouse.Core.Tests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void Create_merges_duplicate_products_into_one_line()
        {
            var box = TestData.Product("BOX", onHand: 50);

            var order = TestData.Order((box, 10), (box, 5));

            var line = Assert.Single(order.Lines);
            Assert.Equal(15, line.Quantity);
            Assert.Equal(OrderStatus.Open, order.Status);
        }

        [Fact]
        public void Create_requires_at_least_one_line()
        {
            var ex = Assert.Throws<BusinessRuleException>(() => TestData.Order());

            Assert.Equal("An order needs at least one line.", ex.Message);
        }

        [Fact]
        public void Create_rejects_non_positive_quantities()
        {
            var box = TestData.Product("BOX", onHand: 50);

            Assert.Throws<BusinessRuleException>(() => TestData.Order((box, 0)));
        }

        [Fact]
        public void Create_does_not_reserve_stock()
        {
            var box = TestData.Product("BOX", onHand: 50);

            TestData.Order((box, 20));

            Assert.Equal(50, box.QuantityAvailable);
        }

        [Fact]
        public void Pick_reserves_stock_for_every_line()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var tape = TestData.Product("TAPE", onHand: 10);
            var order = TestData.Order((box, 20), (tape, 10));

            order.Pick("picker", TestData.Now);

            Assert.Equal(OrderStatus.Picked, order.Status);
            Assert.Equal("picker", order.PickedBy);
            Assert.Equal(TestData.Now, order.PickedAtUtc);
            Assert.Equal(30, box.QuantityAvailable);
            Assert.Equal(0, tape.QuantityAvailable);
        }

        [Fact]
        public void Pick_is_all_or_nothing_when_a_line_is_short()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var scanner = TestData.Product("SCANNER", onHand: 3);
            var order = TestData.Order((box, 20), (scanner, 5));

            var ex = Assert.Throws<BusinessRuleException>(() => order.Pick("picker", TestData.Now));

            Assert.Contains("SCANNER (needs 5, 3 available)", ex.Message);
            Assert.Equal(OrderStatus.Open, order.Status);
            Assert.Equal(0, box.QuantityReserved);
            Assert.Equal(0, scanner.QuantityReserved);
        }

        [Fact]
        public void Two_orders_cannot_reserve_the_same_stock()
        {
            var box = TestData.Product("BOX", onHand: 10);
            var first = TestData.Order((box, 8));
            var second = TestData.Order((box, 8));

            first.Pick("picker", TestData.Now);

            Assert.Throws<BusinessRuleException>(() => second.Pick("picker", TestData.Now));
            Assert.Equal(8, box.QuantityReserved);
        }

        [Fact]
        public void Ship_issues_stock_and_records_ledger_movements()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));
            order.Pick("picker", TestData.Now);
            var carrier = TestData.Carrier();

            var movements = order.Ship(carrier, " TRK-1 ", "shipper", TestData.Now.AddHours(2));

            Assert.Equal(OrderStatus.Shipped, order.Status);
            Assert.Same(carrier, order.Carrier);
            Assert.Equal("TRK-1", order.TrackingNumber);
            Assert.Equal(30, box.QuantityOnHand);
            Assert.Equal(0, box.QuantityReserved);

            var movement = Assert.Single(movements);
            Assert.Equal(StockMovementType.Shipment, movement.Type);
            Assert.Equal(-20, movement.Quantity);
            Assert.Same(box, movement.Product);
        }

        [Fact]
        public void Ship_requires_the_order_to_be_picked_first()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));

            Assert.Throws<BusinessRuleException>(() => order.Ship(TestData.Carrier(), null, "shipper", TestData.Now));
            Assert.Equal(50, box.QuantityOnHand);
        }

        [Fact]
        public void Shipping_twice_is_rejected_and_stock_is_deducted_once()
        {
            // Regression test: the original application deducted stock again on every "Send" click.
            var box = TestData.Product("BOX", onHand: 400);
            var order = TestData.Order((box, 100));
            order.Pick("picker", TestData.Now);
            order.Ship(TestData.Carrier(), null, "shipper", TestData.Now);

            var ex = Assert.Throws<BusinessRuleException>(() => order.Ship(TestData.Carrier(), null, "shipper", TestData.Now));

            Assert.Contains("cannot be shipped", ex.Message);
            Assert.Equal(300, box.QuantityOnHand);
        }

        [Fact]
        public void Ship_requires_a_carrier()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));
            order.Pick("picker", TestData.Now);

            Assert.Throws<BusinessRuleException>(() => order.Ship(null, null, "shipper", TestData.Now));
            Assert.Equal(OrderStatus.Picked, order.Status);
            Assert.Equal(20, box.QuantityReserved);
        }

        [Fact]
        public void Cancel_releases_the_reservation_of_a_picked_order()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));
            order.Pick("picker", TestData.Now);

            order.Cancel("supervisor", TestData.Now);

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.Equal(0, box.QuantityReserved);
            Assert.Equal(50, box.QuantityAvailable);
        }

        [Fact]
        public void Shipped_orders_cannot_be_cancelled()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));
            order.Pick("picker", TestData.Now);
            order.Ship(TestData.Carrier(), null, "shipper", TestData.Now);

            Assert.Throws<BusinessRuleException>(() => order.Cancel("supervisor", TestData.Now));
            Assert.Equal(30, box.QuantityOnHand);
        }

        [Fact]
        public void Available_actions_follow_the_status()
        {
            var box = TestData.Product("BOX", onHand: 50);
            var order = TestData.Order((box, 20));
            Assert.True(order.CanPick && order.CanCancel && !order.CanShip);

            order.Pick("picker", TestData.Now);
            Assert.True(!order.CanPick && order.CanCancel && order.CanShip);

            order.Ship(TestData.Carrier(), null, "shipper", TestData.Now);
            Assert.True(!order.CanPick && !order.CanCancel && !order.CanShip);
            Assert.Equal(20, order.TotalUnits);
            Assert.Equal(new[] { "BOX" }, order.Lines.Select(l => l.Product.Sku));
        }

        [Theory]
        [InlineData(42, "SO-00042")]
        [InlineData(123456, "SO-123456")]
        public void Order_numbers_are_zero_padded(int id, string expected)
        {
            Assert.Equal(expected, Order.FormatNumber(id));
        }
    }
}
