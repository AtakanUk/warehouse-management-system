using System;

namespace Warehouse.Core.Domain
{
    public enum StockMovementType : byte
    {
        /// <summary>Goods received from a supplier (positive quantity).</summary>
        Receipt = 1,

        /// <summary>Goods that left the warehouse on an order (negative quantity).</summary>
        Shipment = 2
    }

    /// <summary>
    /// Immutable entry in the inventory ledger. The quantity on hand of a product always equals
    /// the sum of its movements, which makes every stock level traceable.
    /// </summary>
    public class StockMovement
    {
        public const int ReferenceMaxLength = 50;

        protected StockMovement()
        {
        }

        public int Id { get; private set; }

        public int ProductId { get; private set; }

        public Product Product { get; private set; }

        public StockMovementType Type { get; private set; }

        /// <summary>Signed change of the quantity on hand.</summary>
        public int Quantity { get; private set; }

        /// <summary>Supplier delivery note, purchase order or order number.</summary>
        public string Reference { get; private set; }

        public int? OrderId { get; private set; }

        public Order Order { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public string CreatedBy { get; private set; }

        public static StockMovement ForReceipt(Product product, int quantity, string reference, string receivedBy, DateTime nowUtc)
        {
            Guard.Positive(quantity);
            return new StockMovement
            {
                Product = product ?? throw new ArgumentNullException(nameof(product)),
                ProductId = product.Id,
                Type = StockMovementType.Receipt,
                Quantity = quantity,
                Reference = Guard.Optional(reference, "Reference", ReferenceMaxLength),
                CreatedAtUtc = Guard.Utc(nowUtc),
                CreatedBy = Guard.Required(receivedBy, "User name", Order.UserNameMaxLength)
            };
        }

        internal static StockMovement ForShipment(Product product, int quantity, Order order, string shippedBy, DateTime nowUtc)
        {
            return new StockMovement
            {
                Product = product,
                ProductId = product.Id,
                Type = StockMovementType.Shipment,
                Quantity = -quantity,
                Reference = order.Number,
                Order = order,
                OrderId = order.Id,
                CreatedAtUtc = Guard.Utc(nowUtc),
                CreatedBy = shippedBy
            };
        }
    }
}
