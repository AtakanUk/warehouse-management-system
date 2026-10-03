using System;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Tests
{
    internal static class TestData
    {
        public static readonly DateTime Now = new DateTime(2026, 3, 2, 9, 30, 0, DateTimeKind.Utc);

        public static Product Product(string sku = "SKU-1", int onHand = 0, int reorderLevel = 0)
        {
            var product = global::Warehouse.Core.Domain.Product.Create(
                new ProductDetails { Sku = sku, Name = "Product " + sku, ReorderLevel = reorderLevel });

            if (onHand > 0)
            {
                product.Receive(onHand);
            }

            return product;
        }

        public static Customer Customer(string name = "Rhein Retail GmbH") =>
            global::Warehouse.Core.Domain.Customer.Create(new PartnerDetails { Name = name });

        public static Carrier Carrier(string name = "FastFreight Logistics") =>
            global::Warehouse.Core.Domain.Carrier.Create(new PartnerDetails { Name = name });

        public static Order Order(params (Product Product, int Quantity)[] lines)
        {
            var requests = Array.ConvertAll(lines, l => new OrderLineRequest(l.Product, l.Quantity));
            return global::Warehouse.Core.Domain.Order.Create(Customer(), requests, "PO-1", "tester", Now);
        }
    }
}
