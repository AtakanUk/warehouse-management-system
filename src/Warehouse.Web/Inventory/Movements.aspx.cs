using System;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Inventory
{
    public partial class Movements : WarehousePage
    {
        private string TypeFilter => (Request.QueryString["type"] ?? string.Empty).ToLowerInvariant();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        public IQueryable<StockMovementItem> GetMovements([QueryString("q")] string search)
        {
            StockMovementType? type = null;
            if (TypeFilter == "receipt")
            {
                type = StockMovementType.Receipt;
            }
            else if (TypeFilter == "shipment")
            {
                type = StockMovementType.Shipment;
            }

            return new InventoryService(Db).QueryMovements(type, search);
        }

        protected string TabClass(string type) =>
            string.Equals(TypeFilter, type ?? string.Empty, StringComparison.Ordinal) ? "filter-tab active" : "filter-tab";

        protected string FilterUrl(string type) => BuildUrl(type, Request.QueryString["q"]);

        protected void Search_Click(object sender, EventArgs e) =>
            RedirectTo(BuildUrl(TypeFilter.Length > 0 ? TypeFilter : null, SearchBox.Text.Trim()));

        private string BuildUrl(string type, string search)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(type))
            {
                query["type"] = type;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query["q"] = search;
            }

            return ResolveUrl("~/inventory/movements") + (query.Count > 0 ? "?" + query : string.Empty);
        }
    }
}
