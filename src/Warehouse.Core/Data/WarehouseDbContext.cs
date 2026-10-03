using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Data
{
    /// <summary>
    /// Entity Framework 6 unit of work. The schema is owned by <c>database/schema.sql</c>; this
    /// context only maps to it, so EF never creates or migrates tables on its own.
    /// </summary>
    public class WarehouseDbContext : DbContext
    {
        public const string ConnectionStringName = "WarehouseDb";

        static WarehouseDbContext()
        {
            Database.SetInitializer<WarehouseDbContext>(null);
        }

        public WarehouseDbContext()
            : this("name=" + ConnectionStringName)
        {
        }

        public WarehouseDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            // Aggregates are loaded explicitly with Include(); no lazy loading surprises.
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Carrier> Carriers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderLine> OrderLines { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Saves pending changes. Optimistic concurrency failures and constraint violations that
        /// slipped past the service-level checks (e.g. two users racing) become user-facing errors.
        /// </summary>
        public void Commit()
        {
            try
            {
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException(ex);
            }
            catch (DbUpdateException ex) when (FindSqlError(ex) is int number && (number == 2601 || number == 2627))
            {
                throw new BusinessRuleException("A record with the same unique value already exists.", ex);
            }
            catch (DbUpdateException ex) when (FindSqlError(ex) == 547)
            {
                throw new BusinessRuleException("The change conflicts with related data, for example a record that is still in use.", ex);
            }
        }

        private static int? FindSqlError(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is SqlException sql)
                {
                    return sql.Number;
                }
            }

            return null;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            var product = modelBuilder.Entity<Product>().ToTable("Products");
            product.Property(p => p.Sku).IsRequired().HasMaxLength(Product.SkuMaxLength);
            product.Property(p => p.Name).IsRequired().HasMaxLength(Product.NameMaxLength);
            product.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
            product.Property(p => p.WeightKg).HasPrecision(10, 3);
            product.Property(p => p.Dimensions).HasMaxLength(Product.DimensionsMaxLength);
            product.Property(p => p.CountryOfOrigin).HasMaxLength(2).IsFixedLength().IsUnicode(false);
            product.Property(p => p.RowVersion).IsRowVersion();
            product.Ignore(p => p.QuantityAvailable);
            product.Ignore(p => p.StockStatus);

            var customer = modelBuilder.Entity<Customer>().ToTable("Customers");
            customer.Property(c => c.Name).IsRequired().HasMaxLength(PartnerDetails.NameMaxLength);
            customer.Property(c => c.ContactName).HasMaxLength(PartnerDetails.ContactNameMaxLength);
            customer.Property(c => c.Email).HasMaxLength(254);
            customer.Property(c => c.Phone).HasMaxLength(PartnerDetails.PhoneMaxLength);
            customer.Property(c => c.Address).HasMaxLength(PartnerDetails.AddressMaxLength);

            var carrier = modelBuilder.Entity<Carrier>().ToTable("Carriers");
            carrier.Property(c => c.Name).IsRequired().HasMaxLength(PartnerDetails.NameMaxLength);
            carrier.Property(c => c.ContactName).HasMaxLength(PartnerDetails.ContactNameMaxLength);
            carrier.Property(c => c.Email).HasMaxLength(254);
            carrier.Property(c => c.Phone).HasMaxLength(PartnerDetails.PhoneMaxLength);
            carrier.Property(c => c.Address).HasMaxLength(PartnerDetails.AddressMaxLength);

            var order = modelBuilder.Entity<Order>().ToTable("Orders");
            order.Property(o => o.CustomerReference).HasMaxLength(Order.CustomerReferenceMaxLength);
            order.Property(o => o.TrackingNumber).HasMaxLength(Order.TrackingNumberMaxLength);
            order.Property(o => o.CreatedBy).IsRequired().HasMaxLength(Order.UserNameMaxLength);
            order.Property(o => o.PickedBy).HasMaxLength(Order.UserNameMaxLength);
            order.Property(o => o.ShippedBy).HasMaxLength(Order.UserNameMaxLength);
            order.Property(o => o.CancelledBy).HasMaxLength(Order.UserNameMaxLength);
            order.Property(o => o.CreatedAtUtc).HasColumnType("datetime2");
            order.Property(o => o.PickedAtUtc).HasColumnType("datetime2");
            order.Property(o => o.ShippedAtUtc).HasColumnType("datetime2");
            order.Property(o => o.CancelledAtUtc).HasColumnType("datetime2");
            order.Property(o => o.RowVersion).IsRowVersion();
            order.HasRequired(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId);
            order.HasOptional(o => o.Carrier).WithMany().HasForeignKey(o => o.CarrierId);
            order.HasMany(o => o.Lines).WithRequired(l => l.Order).HasForeignKey(l => l.OrderId);
            order.Ignore(o => o.Number);
            order.Ignore(o => o.TotalUnits);
            order.Ignore(o => o.CanPick);
            order.Ignore(o => o.CanShip);
            order.Ignore(o => o.CanCancel);

            var line = modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            line.HasRequired(l => l.Product).WithMany().HasForeignKey(l => l.ProductId);

            var movement = modelBuilder.Entity<StockMovement>().ToTable("StockMovements");
            movement.Property(m => m.Reference).HasMaxLength(StockMovement.ReferenceMaxLength);
            movement.Property(m => m.CreatedBy).IsRequired().HasMaxLength(Order.UserNameMaxLength);
            movement.Property(m => m.CreatedAtUtc).HasColumnType("datetime2");
            movement.HasRequired(m => m.Product).WithMany().HasForeignKey(m => m.ProductId);
            movement.HasOptional(m => m.Order).WithMany().HasForeignKey(m => m.OrderId);

            var user = modelBuilder.Entity<User>().ToTable("Users");
            user.Property(u => u.UserName).IsRequired().HasMaxLength(User.UserNameMaxLength);
            user.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            user.Property(u => u.CreatedAtUtc).HasColumnType("datetime2");
            user.Property(u => u.LastSignInAtUtc).HasColumnType("datetime2");
            user.Property(u => u.LockoutEndUtc).HasColumnType("datetime2");
        }
    }

    /// <summary>Code-based EF configuration, so no &lt;entityFramework&gt; config section is needed.</summary>
    public class WarehouseDbConfiguration : DbConfiguration
    {
        public WarehouseDbConfiguration()
        {
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }
}
