using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    /// <summary>Maintains the product catalog.</summary>
    public sealed class ProductService
    {
        private readonly WarehouseDbContext _db;

        public ProductService(WarehouseDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IQueryable<ProductListItem> Query(string search = null, bool lowStockOnly = false)
        {
            IQueryable<Product> products = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                products = products.Where(p => p.Sku.Contains(term) || p.Name.Contains(term));
            }

            if (lowStockOnly)
            {
                products = products.Where(p => p.QuantityOnHand - p.QuantityReserved <= p.ReorderLevel);
            }

            return products
                .OrderBy(p => p.Sku)
                .Select(p => new ProductListItem
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityOnHand = p.QuantityOnHand,
                    QuantityReserved = p.QuantityReserved,
                    QuantityAvailable = p.QuantityOnHand - p.QuantityReserved,
                    ReorderLevel = p.ReorderLevel,
                    CanDelete = !_db.OrderLines.Any(l => l.ProductId == p.Id) && !_db.StockMovements.Any(m => m.ProductId == p.Id)
                });
        }

        public IReadOnlyList<ProductOption> GetOptions()
        {
            return _db.Products.AsNoTracking()
                .OrderBy(p => p.Sku)
                .Select(p => new ProductOption
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityAvailable = p.QuantityOnHand - p.QuantityReserved
                })
                .ToList();
        }

        public Product Find(int id) => _db.Products.AsNoTracking().SingleOrDefault(p => p.Id == id);

        public Product Create(ProductDetails details)
        {
            var product = Product.Create(details);
            EnsureSkuIsUnique(product.Sku, excludeId: null);

            _db.Products.Add(product);
            _db.Commit();
            return product;
        }

        /// <param name="id">The product to update.</param>
        /// <param name="details">The new catalog data.</param>
        /// <param name="rowVersion">
        /// The version the user started editing; if the product changed since then the update is
        /// rejected instead of silently overwriting someone else's changes.
        /// </param>
        public void Update(int id, ProductDetails details, byte[] rowVersion)
        {
            var product = _db.Products.Find(id) ?? throw new BusinessRuleException("The product no longer exists.");

            if (rowVersion != null)
            {
                _db.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;
            }

            product.Update(details);
            EnsureSkuIsUnique(product.Sku, excludeId: id);
            _db.Commit();
        }

        public void Delete(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return;
            }

            var hasHistory = _db.OrderLines.Any(l => l.ProductId == id) || _db.StockMovements.Any(m => m.ProductId == id);
            if (hasHistory)
            {
                throw new BusinessRuleException($"{product.Sku} has stock or order history and cannot be deleted.");
            }

            _db.Products.Remove(product);
            _db.Commit();
        }

        private void EnsureSkuIsUnique(string sku, int? excludeId)
        {
            if (_db.Products.Any(p => p.Sku == sku && p.Id != excludeId))
            {
                throw new BusinessRuleException($"A product with SKU {sku} already exists.");
            }
        }
    }
}
