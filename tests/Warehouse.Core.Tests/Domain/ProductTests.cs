using Warehouse.Core.Domain;
using Xunit;

namespace Warehouse.Core.Tests.Domain
{
    public class ProductTests
    {
        [Fact]
        public void Create_normalises_sku_and_country()
        {
            var product = Product.Create(new ProductDetails { Sku = "  pkg-1001 ", Name = " Stretch film ", CountryOfOrigin = "de" });

            Assert.Equal("PKG-1001", product.Sku);
            Assert.Equal("Stretch film", product.Name);
            Assert.Equal("DE", product.CountryOfOrigin);
        }

        [Theory]
        [InlineData("", "Name")]
        [InlineData("SKU 1", "Name")]
        [InlineData("SKU-1", "")]
        public void Create_rejects_invalid_details(string sku, string name)
        {
            Assert.Throws<BusinessRuleException>(() => Product.Create(new ProductDetails { Sku = sku, Name = name }));
        }

        [Fact]
        public void Create_rejects_invalid_country_code()
        {
            var ex = Assert.Throws<BusinessRuleException>(() =>
                Product.Create(new ProductDetails { Sku = "SKU-1", Name = "Name", CountryOfOrigin = "D1" }));

            Assert.Contains("two-letter", ex.Message);
        }

        [Fact]
        public void Receive_increases_quantity_on_hand()
        {
            var product = TestData.Product(onHand: 10);

            product.Receive(5);

            Assert.Equal(15, product.QuantityOnHand);
            Assert.Equal(15, product.QuantityAvailable);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Receive_rejects_non_positive_quantities(int quantity)
        {
            var product = TestData.Product(onHand: 10);

            Assert.Throws<BusinessRuleException>(() => product.Receive(quantity));
            Assert.Equal(10, product.QuantityOnHand);
        }

        [Fact]
        public void Reserve_reduces_available_but_not_on_hand()
        {
            var product = TestData.Product(onHand: 10);

            product.Reserve(4);

            Assert.Equal(10, product.QuantityOnHand);
            Assert.Equal(4, product.QuantityReserved);
            Assert.Equal(6, product.QuantityAvailable);
        }

        [Fact]
        public void Reserve_cannot_exceed_available_stock()
        {
            var product = TestData.Product(onHand: 10);
            product.Reserve(8);

            var ex = Assert.Throws<BusinessRuleException>(() => product.Reserve(3));

            Assert.Contains("Only 2 unit(s)", ex.Message);
            Assert.Equal(8, product.QuantityReserved);
        }

        [Fact]
        public void Issue_removes_reserved_stock_from_the_warehouse()
        {
            var product = TestData.Product(onHand: 10);
            product.Reserve(4);

            product.Issue(4);

            Assert.Equal(6, product.QuantityOnHand);
            Assert.Equal(0, product.QuantityReserved);
        }

        [Fact]
        public void Issue_requires_a_reservation()
        {
            var product = TestData.Product(onHand: 10);

            Assert.Throws<BusinessRuleException>(() => product.Issue(1));
        }

        [Theory]
        [InlineData(0, 5, StockStatus.OutOfStock)]
        [InlineData(5, 5, StockStatus.Low)]
        [InlineData(6, 5, StockStatus.InStock)]
        public void Stock_status_compares_available_quantity_with_reorder_level(int available, int reorderLevel, StockStatus expected)
        {
            Assert.Equal(expected, Product.GetStockStatus(available, reorderLevel));
        }
    }
}
