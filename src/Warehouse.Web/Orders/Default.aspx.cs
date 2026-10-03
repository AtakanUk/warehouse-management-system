using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Orders
{
    public partial class OrderList : WarehousePage
    {
        private IReadOnlyDictionary<OrderStatus, int> _counts;

        private string StatusFilter => (Request.QueryString["status"] ?? string.Empty).ToLowerInvariant();

        private IReadOnlyDictionary<OrderStatus, int> Counts => _counts ?? (_counts = new OrderService(Db).GetStatusCounts());

        protected int TotalCount => Counts.Values.Sum();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        public IQueryable<OrderListItem> GetOrders([QueryString("q")] string search)
        {
            OrderStatus? status = Enum.TryParse(StatusFilter, ignoreCase: true, out OrderStatus parsed) && Enum.IsDefined(typeof(OrderStatus), parsed)
                ? parsed
                : (OrderStatus?)null;

            return new OrderService(Db).Query(status, search);
        }

        protected int CountOf(OrderStatus status) => Counts[status];

        protected string TabClass(string status) =>
            string.Equals(StatusFilter, status ?? string.Empty, StringComparison.Ordinal) ? "filter-tab active" : "filter-tab";

        protected string FilterUrl(string status) => BuildUrl(status, Request.QueryString["q"]);

        protected void Search_Click(object sender, EventArgs e) =>
            RedirectTo(BuildUrl(StatusFilter.Length > 0 ? StatusFilter : null, SearchBox.Text.Trim()));

        private string BuildUrl(string status, string search)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(status))
            {
                query["status"] = status;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query["q"] = search;
            }

            return ResolveUrl("~/orders") + (query.Count > 0 ? "?" + query : string.Empty);
        }
    }
}
