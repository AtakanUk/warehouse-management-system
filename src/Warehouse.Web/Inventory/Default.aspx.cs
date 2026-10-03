using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Inventory
{
    public partial class StockLevels : WarehousePage
    {
        protected bool LowStockOnly => string.Equals(Request.QueryString["filter"], "low", StringComparison.OrdinalIgnoreCase);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        public IQueryable<ProductListItem> GetStockLevels([QueryString("q")] string search) =>
            new ProductService(Db).Query(search, LowStockOnly);

        /// <summary>Available stock relative to twice the reorder level (the "healthy" range).</summary>
        protected static string BarWidth(ProductListItem item)
        {
            var scale = Math.Max(1, Math.Max(item.ReorderLevel * 2, item.QuantityAvailable));
            return Percent(Math.Max(0, item.QuantityAvailable) * 100.0 / scale);
        }

        protected static string MarkerPosition(ProductListItem item)
        {
            var scale = Math.Max(1, Math.Max(item.ReorderLevel * 2, item.QuantityAvailable));
            return Percent(item.ReorderLevel * 100.0 / scale);
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (LowStockOnly)
            {
                query["filter"] = "low";
            }

            if (SearchBox.Text.Trim().Length > 0)
            {
                query["q"] = SearchBox.Text.Trim();
            }

            RedirectTo("~/inventory" + (query.Count > 0 ? "?" + query : string.Empty));
        }

        private static string Percent(double value) => Math.Min(100, value).ToString("0.#", CultureInfo.InvariantCulture);
    }
}
