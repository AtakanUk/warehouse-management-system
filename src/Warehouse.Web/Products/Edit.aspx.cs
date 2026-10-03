using System;
using System.Globalization;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Products
{
    public partial class ProductEdit : WarehousePage
    {
        private const string RowVersionKey = "RowVersion";

        protected bool IsNew => RouteId == null;

        protected int OnHand { get; private set; }

        protected int Reserved { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Title = IsNew ? "New product" : "Edit product";

            if (IsPostBack || IsNew)
            {
                return;
            }

            var product = new ProductService(Db).Find(RouteId.Value);
            if (product == null)
            {
                ShowNotFound();
                return;
            }

            SkuBox.Text = product.Sku;
            NameBox.Text = product.Name;
            DescriptionBox.Text = product.Description;
            WeightBox.Text = product.WeightKg?.ToString("0.###", CultureInfo.CurrentCulture);
            DimensionsBox.Text = product.Dimensions;
            CountryBox.Text = product.CountryOfOrigin;
            ReorderLevelBox.Text = product.ReorderLevel.ToString(CultureInfo.InvariantCulture);

            // Remember the version the user starts from (optimistic concurrency).
            ViewState[RowVersionKey] = Convert.ToBase64String(product.RowVersion);
            Title = "Edit " + product.Sku;
            ViewState["Title"] = Title;
            ViewState["OnHand"] = product.QuantityOnHand;
            ViewState["Reserved"] = product.QuantityReserved;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!IsNew)
            {
                Title = ViewState["Title"] as string ?? Title;
                OnHand = ViewState["OnHand"] as int? ?? 0;
                Reserved = ViewState["Reserved"] as int? ?? 0;
                StockPanel.Visible = true;
            }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var details = new ProductDetails
            {
                Sku = SkuBox.Text,
                Name = NameBox.Text,
                Description = DescriptionBox.Text,
                WeightKg = decimal.TryParse(WeightBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var weight) ? weight : (decimal?)null,
                Dimensions = DimensionsBox.Text,
                CountryOfOrigin = CountryBox.Text,
                ReorderLevel = int.Parse(ReorderLevelBox.Text, CultureInfo.InvariantCulture)
            };

            var products = new ProductService(Db);
            if (IsNew)
            {
                Product created = null;
                if (TryRun(() => created = products.Create(details)))
                {
                    RedirectWithMessage("~/products", $"Product {created.Sku} created. Book its first delivery under Receive goods.");
                }
            }
            else
            {
                var rowVersion = ViewState[RowVersionKey] is string version ? Convert.FromBase64String(version) : null;
                if (TryRun(() => products.Update(RouteId.Value, details, rowVersion)))
                {
                    RedirectWithMessage("~/products", $"Product {details.Sku.Trim().ToUpperInvariant()} updated.");
                }
            }
        }
    }
}
