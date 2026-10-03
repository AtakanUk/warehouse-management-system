using System.Text.RegularExpressions;

namespace Warehouse.Core.Domain
{
    /// <summary>
    /// A stock-keeping unit together with its inventory position.
    /// </summary>
    /// <remarks>
    /// Stock is tracked as two numbers: <see cref="QuantityOnHand"/> is what is physically in the
    /// warehouse, <see cref="QuantityReserved"/> is the part of it already allocated to picked
    /// orders. Only <see cref="QuantityAvailable"/> can be promised to new orders.
    /// </remarks>
    public class Product
    {
        public const int SkuMaxLength = 32;
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int DimensionsMaxLength = 50;

        private static readonly Regex SkuPattern = new Regex(@"^[A-Z0-9][A-Z0-9\-_.]*$", RegexOptions.Compiled);
        private static readonly Regex CountryPattern = new Regex(@"^[A-Z]{2}$", RegexOptions.Compiled);

        protected Product()
        {
        }

        public int Id { get; private set; }

        public string Sku { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public decimal? WeightKg { get; private set; }

        public string Dimensions { get; private set; }

        /// <summary>ISO 3166-1 alpha-2 country code.</summary>
        public string CountryOfOrigin { get; private set; }

        /// <summary>When available stock drops to this level the product needs replenishment.</summary>
        public int ReorderLevel { get; private set; }

        public int QuantityOnHand { get; private set; }

        public int QuantityReserved { get; private set; }

        public byte[] RowVersion { get; private set; }

        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        public StockStatus StockStatus => GetStockStatus(QuantityAvailable, ReorderLevel);

        public static StockStatus GetStockStatus(int available, int reorderLevel)
        {
            if (available <= 0)
            {
                return StockStatus.OutOfStock;
            }

            return available <= reorderLevel ? StockStatus.Low : StockStatus.InStock;
        }

        public static Product Create(ProductDetails details)
        {
            var product = new Product();
            product.Update(details);
            return product;
        }

        public void Update(ProductDetails details)
        {
            if (details == null)
            {
                throw new System.ArgumentNullException(nameof(details));
            }

            var sku = Guard.UpperInvariant(Guard.Required(details.Sku, "SKU", SkuMaxLength));
            if (!SkuPattern.IsMatch(sku))
            {
                throw new BusinessRuleException("SKU may only contain letters, digits, '-', '_' and '.'.");
            }

            var country = Guard.UpperInvariant(Guard.Optional(details.CountryOfOrigin, "Country of origin", 2));
            if (country != null && !CountryPattern.IsMatch(country))
            {
                throw new BusinessRuleException("Country of origin must be a two-letter ISO code, for example DE.");
            }

            if (details.WeightKg < 0)
            {
                throw new BusinessRuleException("Weight cannot be negative.");
            }

            Guard.NotNegative(details.ReorderLevel, "Reorder level");

            Sku = sku;
            Name = Guard.Required(details.Name, "Name", NameMaxLength);
            Description = Guard.Optional(details.Description, "Description", DescriptionMaxLength);
            WeightKg = details.WeightKg;
            Dimensions = Guard.Optional(details.Dimensions, "Dimensions", DimensionsMaxLength);
            CountryOfOrigin = country;
            ReorderLevel = details.ReorderLevel;
        }

        /// <summary>Books goods that arrived at the warehouse.</summary>
        public void Receive(int quantity)
        {
            Guard.Positive(quantity);
            QuantityOnHand += quantity;
        }

        /// <summary>Allocates available stock to an order that is being picked.</summary>
        public void Reserve(int quantity)
        {
            Guard.Positive(quantity);
            if (quantity > QuantityAvailable)
            {
                throw new BusinessRuleException(
                    $"Only {QuantityAvailable} unit(s) of {Sku} are available, {quantity} requested.");
            }

            QuantityReserved += quantity;
        }

        /// <summary>Returns previously reserved stock to the available pool (order cancelled).</summary>
        public void Release(int quantity)
        {
            Guard.Positive(quantity);
            if (quantity > QuantityReserved)
            {
                throw new BusinessRuleException($"Cannot release more than the {QuantityReserved} unit(s) reserved for {Sku}.");
            }

            QuantityReserved -= quantity;
        }

        /// <summary>Removes reserved stock from the warehouse when the order leaves the dock.</summary>
        public void Issue(int quantity)
        {
            Guard.Positive(quantity);
            if (quantity > QuantityReserved)
            {
                throw new BusinessRuleException($"Cannot ship {quantity} unit(s) of {Sku}: only {QuantityReserved} are reserved.");
            }

            QuantityReserved -= quantity;
            QuantityOnHand -= quantity;
        }
    }

    public enum StockStatus
    {
        InStock,
        Low,
        OutOfStock
    }

    /// <summary>Editable catalog data of a product.</summary>
    public class ProductDetails
    {
        public string Sku { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal? WeightKg { get; set; }

        public string Dimensions { get; set; }

        public string CountryOfOrigin { get; set; }

        public int ReorderLevel { get; set; }
    }
}
