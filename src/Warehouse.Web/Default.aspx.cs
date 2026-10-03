using System;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web
{
    public partial class DefaultPage : WarehousePage
    {
        protected DashboardSummary Summary { get; private set; } = new DashboardSummary();

        protected string DayPart
        {
            get
            {
                var hour = DateTime.Now.Hour;
                return hour < 12 ? "morning" : hour < 18 ? "afternoon" : "evening";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Summary = new DashboardService(Db).GetSummary();

            AwaitingOrders.DataSource = Summary.OrdersAwaitingAction;
            AwaitingOrders.DataBind();
            NoAwaitingOrders.Visible = Summary.OrdersAwaitingAction.Count == 0;

            LowStock.DataSource = Summary.LowStock;
            LowStock.DataBind();
            NoLowStock.Visible = Summary.LowStock.Count == 0;

            RecentMovements.DataSource = Summary.RecentMovements;
            RecentMovements.DataBind();
        }
    }
}
