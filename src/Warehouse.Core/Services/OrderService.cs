using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    /// <summary>Outbound order workflow: capture, pick, ship and cancel.</summary>
    public sealed class OrderService
    {
        private readonly WarehouseDbContext _db;
        private readonly Func<DateTime> _utcNow;

        public OrderService(WarehouseDbContext db, Func<DateTime> utcNow = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public Order Create(int customerId, IEnumerable<OrderLineInput> lines, string customerReference, string createdBy)
        {
            var customer = _db.Customers.Find(customerId) ?? throw new BusinessRuleException("Select a customer for the order.");
            var input = (lines ?? Enumerable.Empty<OrderLineInput>()).ToList();

            var productIds = input.Select(l => l.ProductId).Distinct().ToList();
            var products = _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionary(p => p.Id);

            var requests = input.Select(l => products.TryGetValue(l.ProductId, out var product)
                ? new OrderLineRequest(product, l.Quantity)
                : throw new BusinessRuleException("One of the selected products no longer exists."));

            var order = Order.Create(customer, requests, customerReference, createdBy, _utcNow());
            _db.Orders.Add(order);
            _db.Commit();
            return order;
        }

        /// <summary>Loads the full aggregate (customer, carrier, lines and their products).</summary>
        public Order Find(int id) =>
            _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Carrier)
                .Include(o => o.Lines.Select(l => l.Product))
                .SingleOrDefault(o => o.Id == id);

        public void Pick(int id, string pickedBy)
        {
            Get(id).Pick(pickedBy, _utcNow());
            _db.Commit();
        }

        public void Ship(int id, int carrierId, string trackingNumber, string shippedBy)
        {
            var order = Get(id);
            var carrier = _db.Carriers.Find(carrierId);

            var movements = order.Ship(carrier, trackingNumber, shippedBy, _utcNow());
            _db.StockMovements.AddRange(movements);
            _db.Commit();
        }

        public void Cancel(int id, string cancelledBy)
        {
            Get(id).Cancel(cancelledBy, _utcNow());
            _db.Commit();
        }

        public IQueryable<OrderListItem> Query(OrderStatus? status = null, string search = null)
        {
            IQueryable<Order> orders = _db.Orders.AsNoTracking();

            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var number = ParseOrderNumber(term);
                orders = orders.Where(o => o.Id == number || o.Customer.Name.Contains(term) || o.CustomerReference.Contains(term));
            }

            return orders
                .OrderByDescending(o => o.Id)
                .Select(o => new OrderListItem
                {
                    Id = o.Id,
                    CustomerName = o.Customer.Name,
                    CustomerReference = o.CustomerReference,
                    Status = o.Status,
                    CreatedAtUtc = o.CreatedAtUtc,
                    ShippedAtUtc = o.ShippedAtUtc,
                    CarrierName = o.Carrier.Name,
                    LineCount = o.Lines.Count(),
                    TotalUnits = o.Lines.Sum(l => (int?)l.Quantity) ?? 0
                });
        }

        /// <summary>Number of orders per status, including statuses without orders.</summary>
        public IReadOnlyDictionary<OrderStatus, int> GetStatusCounts()
        {
            var counts = _db.Orders.AsNoTracking()
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Status, x => x.Count);

            return Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>()
                .ToDictionary(status => status, status => counts.TryGetValue(status, out var count) ? count : 0);
        }

        /// <summary>Open orders, oldest first, with a live check against available stock.</summary>
        public IQueryable<PickingQueueItem> QueryPickingQueue()
        {
            return _db.Orders.AsNoTracking()
                .Where(o => o.Status == OrderStatus.Open)
                .OrderBy(o => o.CreatedAtUtc)
                .ThenBy(o => o.Id)
                .Select(o => new PickingQueueItem
                {
                    Id = o.Id,
                    CustomerName = o.Customer.Name,
                    CreatedAtUtc = o.CreatedAtUtc,
                    LineCount = o.Lines.Count(),
                    TotalUnits = o.Lines.Sum(l => (int?)l.Quantity) ?? 0,
                    ShortLineCount = o.Lines.Count(l => l.Quantity > l.Product.QuantityOnHand - l.Product.QuantityReserved)
                });
        }

        /// <summary>Picked orders waiting for a carrier, oldest first.</summary>
        public IQueryable<ShippingQueueItem> QueryShippingQueue()
        {
            return _db.Orders.AsNoTracking()
                .Where(o => o.Status == OrderStatus.Picked)
                .OrderBy(o => o.PickedAtUtc)
                .ThenBy(o => o.Id)
                .Select(o => new ShippingQueueItem
                {
                    Id = o.Id,
                    CustomerName = o.Customer.Name,
                    PickedAtUtc = o.PickedAtUtc,
                    PickedBy = o.PickedBy,
                    LineCount = o.Lines.Count(),
                    TotalUnits = o.Lines.Sum(l => (int?)l.Quantity) ?? 0
                });
        }

        /// <summary>Accepts "SO-00042", "so42" or "42"; returns 0 when the text is not an order number.</summary>
        internal static int ParseOrderNumber(string text)
        {
            var digits = text.StartsWith("SO", StringComparison.OrdinalIgnoreCase) ? text.Substring(2).TrimStart('-', ' ') : text;
            return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : 0;
        }

        private Order Get(int id) => Find(id) ?? throw new BusinessRuleException("The order no longer exists.");
    }
}
