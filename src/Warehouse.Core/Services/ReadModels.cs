using System;
using System.Collections.Generic;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    // Flat projections used by list screens. They are materialised straight from SQL, so paging
    // and sorting on their properties is executed by the database.

    public class ProductListItem
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public int QuantityOnHand { get; set; }

        public int QuantityReserved { get; set; }

        public int QuantityAvailable { get; set; }

        public int ReorderLevel { get; set; }

        /// <summary>False once the product has order lines or ledger entries.</summary>
        public bool CanDelete { get; set; }

        public StockStatus StockStatus => Product.GetStockStatus(QuantityAvailable, ReorderLevel);
    }

    public class ProductOption
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public int QuantityAvailable { get; set; }

        public string DisplayName => $"{Sku} · {Name} ({QuantityAvailable} available)";
    }

    public class PartnerOption
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class PartnerListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ContactName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        /// <summary>Orders placed (customers) or shipments handled (carriers).</summary>
        public int OrderCount { get; set; }
    }

    public class StockMovementItem
    {
        public int Id { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public int ProductId { get; set; }

        public string Sku { get; set; }

        public string ProductName { get; set; }

        public StockMovementType Type { get; set; }

        public int Quantity { get; set; }

        public string Reference { get; set; }

        public int? OrderId { get; set; }

        public string CreatedBy { get; set; }
    }

    public class OrderListItem
    {
        public int Id { get; set; }

        public string Number => Order.FormatNumber(Id);

        public string CustomerName { get; set; }

        public string CustomerReference { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ShippedAtUtc { get; set; }

        public string CarrierName { get; set; }

        public int LineCount { get; set; }

        public int TotalUnits { get; set; }
    }

    public class PickingQueueItem
    {
        public int Id { get; set; }

        public string Number => Order.FormatNumber(Id);

        public string CustomerName { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public int LineCount { get; set; }

        public int TotalUnits { get; set; }

        /// <summary>Lines whose quantity exceeds the currently available stock.</summary>
        public int ShortLineCount { get; set; }

        public bool IsReady => ShortLineCount == 0;
    }

    public class ShippingQueueItem
    {
        public int Id { get; set; }

        public string Number => Order.FormatNumber(Id);

        public string CustomerName { get; set; }

        public DateTime? PickedAtUtc { get; set; }

        public string PickedBy { get; set; }

        public int LineCount { get; set; }

        public int TotalUnits { get; set; }
    }

    public class DashboardSummary
    {
        public int ProductCount { get; set; }

        public int UnitsOnHand { get; set; }

        public int UnitsReserved { get; set; }

        public int LowStockCount { get; set; }

        public int OpenOrders { get; set; }

        public int PickedOrders { get; set; }

        public int ShippedLast7Days { get; set; }

        public IReadOnlyList<ProductListItem> LowStock { get; set; } = Array.Empty<ProductListItem>();

        public IReadOnlyList<OrderListItem> OrdersAwaitingAction { get; set; } = Array.Empty<OrderListItem>();

        public IReadOnlyList<StockMovementItem> RecentMovements { get; set; } = Array.Empty<StockMovementItem>();
    }

    /// <summary>A product and quantity entered on the new order screen.</summary>
    public class OrderLineInput
    {
        public OrderLineInput(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public int ProductId { get; }

        public int Quantity { get; }
    }
}
