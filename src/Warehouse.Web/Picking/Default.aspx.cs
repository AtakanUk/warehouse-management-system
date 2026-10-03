using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Picking
{
    public partial class PickingQueue : WarehousePage
    {
        public IQueryable<PickingQueueItem> GetQueue() => new OrderService(Db).QueryPickingQueue();

        protected void Queue_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (e.CommandName != "Pick")
            {
                return;
            }

            var orderId = int.Parse((string)e.CommandArgument, CultureInfo.InvariantCulture);
            if (TryRun(() => new OrderService(Db).Pick(orderId, CurrentUserName)))
            {
                RedirectWithMessage("~/picking", $"{Order.FormatNumber(orderId)} picked and moved to the shipping queue.");
            }
            else
            {
                QueueGrid.DataBind();
            }
        }
    }
}
