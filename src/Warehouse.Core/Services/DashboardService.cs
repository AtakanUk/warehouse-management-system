using System;
using System.Data.Entity;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    /// <summary>Key figures for the start page.</summary>
    public sealed class DashboardService
    {
        private readonly WarehouseDbContext _db;
        private readonly Func<DateTime> _utcNow;

        public DashboardService(WarehouseDbContext db, Func<DateTime> utcNow = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public DashboardSummary GetSummary()
        {
            var products = _db.Products.AsNoTracking();
            var orders = _db.Orders.AsNoTracking();
            var weekAgo = _utcNow().AddDays(-7);

            return new DashboardSummary
            {
                ProductCount = products.Count(),
                UnitsOnHand = products.Sum(p => (int?)p.QuantityOnHand) ?? 0,
                UnitsReserved = products.Sum(p => (int?)p.QuantityReserved) ?? 0,
                LowStockCount = products.Count(p => p.QuantityOnHand - p.QuantityReserved <= p.ReorderLevel),
                OpenOrders = orders.Count(o => o.Status == OrderStatus.Open),
                PickedOrders = orders.Count(o => o.Status == OrderStatus.Picked),
                ShippedLast7Days = orders.Count(o => o.Status == OrderStatus.Shipped && o.ShippedAtUtc >= weekAgo),

                LowStock = new ProductService(_db).Query(lowStockOnly: true)
                    .OrderBy(p => p.QuantityAvailable - p.ReorderLevel)
                    .ThenBy(p => p.Sku)
                    .Take(5)
                    .ToList(),

                OrdersAwaitingAction = new OrderService(_db, _utcNow).Query()
                    .Where(o => o.Status == OrderStatus.Open || o.Status == OrderStatus.Picked)
                    .OrderBy(o => o.CreatedAtUtc)
                    .Take(5)
                    .ToList(),

                RecentMovements = new InventoryService(_db, _utcNow).GetRecentMovements(6)
            };
        }
    }
}
