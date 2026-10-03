using System;
using System.Collections.Generic;
using System.Globalization;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Inventory
{
    public partial class Receive : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ProductList.DataBind();
                var preselected = Request.QueryString["product"];
                if (ProductList.Items.FindByValue(preselected ?? string.Empty) != null)
                {
                    ProductList.SelectedValue = preselected;
                    QuantityBox.Focus();
                }
            }
        }

        public IReadOnlyList<ProductOption> GetProducts() => new ProductService(Db).GetOptions();

        public IReadOnlyList<StockMovementItem> GetRecentReceipts() =>
            new InventoryService(Db).GetRecentMovements(8, StockMovementType.Receipt);

        protected void Receive_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var productId = int.Parse(ProductList.SelectedValue, CultureInfo.InvariantCulture);
            var quantity = int.Parse(QuantityBox.Text, CultureInfo.InvariantCulture);

            StockMovement receipt = null;
            if (TryRun(() => receipt = new InventoryService(Db).Receive(productId, quantity, ReferenceBox.Text, CurrentUserName)))
            {
                RedirectWithMessage("~/inventory/receive",
                    $"Received {Display.Number(quantity)} × {receipt.Product.Sku}. {receipt.Product.Sku} now has {Display.Number(receipt.Product.QuantityAvailable)} available.");
            }
        }
    }
}
