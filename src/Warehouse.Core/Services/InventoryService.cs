using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    /// <summary>Goods receipt and the inventory ledger.</summary>
    public sealed class InventoryService
    {
        private readonly WarehouseDbContext _db;
        private readonly Func<DateTime> _utcNow;

        public InventoryService(WarehouseDbContext db, Func<DateTime> utcNow = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        /// <summary>Books a delivery: increases the quantity on hand and records a ledger entry.</summary>
        public StockMovement Receive(int productId, int quantity, string reference, string receivedBy)
        {
            var product = _db.Products.Find(productId) ?? throw new BusinessRuleException("Select a product to receive.");

            product.Receive(quantity);
            var movement = StockMovement.ForReceipt(product, quantity, reference, receivedBy, _utcNow());
            _db.StockMovements.Add(movement);
            _db.Commit();
            return movement;
        }

        public IQueryable<StockMovementItem> QueryMovements(StockMovementType? type = null, string search = null)
        {
            IQueryable<StockMovement> movements = _db.StockMovements.AsNoTracking();

            if (type.HasValue)
            {
                movements = movements.Where(m => m.Type == type.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                movements = movements.Where(m => m.Product.Sku.Contains(term) || m.Product.Name.Contains(term) || m.Reference.Contains(term));
            }

            return movements
                .OrderByDescending(m => m.CreatedAtUtc)
                .ThenByDescending(m => m.Id)
                .Select(m => new StockMovementItem
                {
                    Id = m.Id,
                    CreatedAtUtc = m.CreatedAtUtc,
                    ProductId = m.ProductId,
                    Sku = m.Product.Sku,
                    ProductName = m.Product.Name,
                    Type = m.Type,
                    Quantity = m.Quantity,
                    Reference = m.Reference,
                    OrderId = m.OrderId,
                    CreatedBy = m.CreatedBy
                });
        }

        public IReadOnlyList<StockMovementItem> GetRecentMovements(int count, StockMovementType? type = null) =>
            QueryMovements(type).Take(count).ToList();
    }
}
