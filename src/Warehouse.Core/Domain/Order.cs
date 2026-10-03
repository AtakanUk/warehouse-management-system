using System;
using System.Collections.Generic;
using System.Linq;

namespace Warehouse.Core.Domain
{
    /// <summary>Lifecycle of an outbound order.</summary>
    public enum OrderStatus : byte
    {
        /// <summary>Captured, no stock allocated yet.</summary>
        Open = 0,

        /// <summary>All lines picked; the stock is reserved for this order.</summary>
        Picked = 1,

        /// <summary>Handed over to a carrier; the stock has left the warehouse.</summary>
        Shipped = 2,

        /// <summary>Withdrawn; any reservation has been released.</summary>
        Cancelled = 3
    }

    /// <summary>A product and quantity requested on a new order.</summary>
    public sealed class OrderLineRequest
    {
        public OrderLineRequest(Product product, int quantity)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity;
        }

        public Product Product { get; }

        public int Quantity { get; }
    }

    /// <summary>
    /// Outbound order aggregate. All stock changes caused by an order go through this class so the
    /// state machine <c>Open → Picked → Shipped</c> (or <c>Cancelled</c>) is enforced in one place.
    /// </summary>
    public class Order
    {
        public const int CustomerReferenceMaxLength = 50;
        public const int TrackingNumberMaxLength = 50;
        public const int UserNameMaxLength = 50;

        protected Order()
        {
        }

        public int Id { get; private set; }

        public int CustomerId { get; private set; }

        public Customer Customer { get; private set; }

        public OrderStatus Status { get; private set; }

        /// <summary>The customer's own reference, typically their purchase order number.</summary>
        public string CustomerReference { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public string CreatedBy { get; private set; }

        public DateTime? PickedAtUtc { get; private set; }

        public string PickedBy { get; private set; }

        public DateTime? ShippedAtUtc { get; private set; }

        public string ShippedBy { get; private set; }

        public DateTime? CancelledAtUtc { get; private set; }

        public string CancelledBy { get; private set; }

        public int? CarrierId { get; private set; }

        public Carrier Carrier { get; private set; }

        public string TrackingNumber { get; private set; }

        public byte[] RowVersion { get; private set; }

        public ICollection<OrderLine> Lines { get; private set; } = new List<OrderLine>();

        public string Number => FormatNumber(Id);

        public int TotalUnits => Lines.Sum(line => line.Quantity);

        public bool CanPick => Status == OrderStatus.Open;

        public bool CanShip => Status == OrderStatus.Picked;

        public bool CanCancel => Status == OrderStatus.Open || Status == OrderStatus.Picked;

        public static string FormatNumber(int id) => "SO-" + id.ToString("D5", System.Globalization.CultureInfo.InvariantCulture);

        public static Order Create(Customer customer, IEnumerable<OrderLineRequest> lines, string customerReference, string createdBy, DateTime nowUtc)
        {
            if (customer == null)
            {
                throw new BusinessRuleException("Select a customer for the order.");
            }

            var requested = (lines ?? Enumerable.Empty<OrderLineRequest>()).ToList();
            if (requested.Count == 0)
            {
                throw new BusinessRuleException("An order needs at least one line.");
            }

            foreach (var line in requested)
            {
                Guard.Positive(line.Quantity, $"Quantity for {line.Product.Sku}");
            }

            var order = new Order
            {
                Customer = customer,
                CustomerId = customer.Id,
                Status = OrderStatus.Open,
                CustomerReference = Guard.Optional(customerReference, "Customer reference", CustomerReferenceMaxLength),
                CreatedAtUtc = Guard.Utc(nowUtc),
                CreatedBy = RequireUser(createdBy)
            };

            // The same product entered twice becomes one line with the combined quantity.
            foreach (var group in requested.GroupBy(line => line.Product))
            {
                order.Lines.Add(new OrderLine(order, group.Key, group.Sum(line => line.Quantity)));
            }

            return order;
        }

        /// <summary>
        /// Confirms that every line can be picked and reserves the stock. The operation is
        /// all-or-nothing: if any line is short, nothing is reserved.
        /// </summary>
        public void Pick(string pickedBy, DateTime nowUtc)
        {
            EnsureStatus(OrderStatus.Open, "picked");

            var shortages = Lines
                .Where(line => line.Product.QuantityAvailable < line.Quantity)
                .Select(line => $"{line.Product.Sku} (needs {line.Quantity}, {Math.Max(0, line.Product.QuantityAvailable)} available)")
                .ToList();

            if (shortages.Count > 0)
            {
                throw new BusinessRuleException($"{Number} cannot be picked, stock is short for: {string.Join(", ", shortages)}.");
            }

            foreach (var line in Lines)
            {
                line.Product.Reserve(line.Quantity);
            }

            Status = OrderStatus.Picked;
            PickedAtUtc = Guard.Utc(nowUtc);
            PickedBy = RequireUser(pickedBy);
        }

        /// <summary>
        /// Hands the order to a carrier: reserved stock leaves the warehouse and one stock movement
        /// per line is produced for the inventory ledger.
        /// </summary>
        public IReadOnlyList<StockMovement> Ship(Carrier carrier, string trackingNumber, string shippedBy, DateTime nowUtc)
        {
            EnsureStatus(OrderStatus.Picked, "shipped");

            if (carrier == null)
            {
                throw new BusinessRuleException("Select a carrier for the shipment.");
            }

            var tracking = Guard.Optional(trackingNumber, "Tracking number", TrackingNumberMaxLength);
            var user = RequireUser(shippedBy);
            var movements = new List<StockMovement>();

            foreach (var line in Lines)
            {
                line.Product.Issue(line.Quantity);
                movements.Add(StockMovement.ForShipment(line.Product, line.Quantity, this, user, nowUtc));
            }

            Carrier = carrier;
            CarrierId = carrier.Id;
            TrackingNumber = tracking;
            Status = OrderStatus.Shipped;
            ShippedAtUtc = Guard.Utc(nowUtc);
            ShippedBy = user;
            return movements;
        }

        /// <summary>Withdraws the order and releases any stock reserved for it.</summary>
        public void Cancel(string cancelledBy, DateTime nowUtc)
        {
            if (!CanCancel)
            {
                throw new BusinessRuleException($"{Number} is {Status.ToString().ToLowerInvariant()} and can no longer be cancelled.");
            }

            if (Status == OrderStatus.Picked)
            {
                foreach (var line in Lines)
                {
                    line.Product.Release(line.Quantity);
                }
            }

            Status = OrderStatus.Cancelled;
            CancelledAtUtc = Guard.Utc(nowUtc);
            CancelledBy = RequireUser(cancelledBy);
        }

        private void EnsureStatus(OrderStatus expected, string action)
        {
            if (Status != expected)
            {
                throw new BusinessRuleException(
                    $"{Number} is {Status.ToString().ToLowerInvariant()} and cannot be {action}. Only {expected.ToString().ToLowerInvariant()} orders can be {action}.");
            }
        }

        private static string RequireUser(string userName) =>
            Guard.Required(userName, "User name", UserNameMaxLength);
    }

    /// <summary>One product on an order.</summary>
    public class OrderLine
    {
        protected OrderLine()
        {
        }

        internal OrderLine(Order order, Product product, int quantity)
        {
            Order = order;
            Product = product;
            ProductId = product.Id;
            Quantity = quantity;
        }

        public int Id { get; private set; }

        public int OrderId { get; private set; }

        public Order Order { get; private set; }

        public int ProductId { get; private set; }

        public Product Product { get; private set; }

        public int Quantity { get; private set; }
    }
}
