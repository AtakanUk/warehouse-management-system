using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Shipping
{
    public partial class ShippingQueue : WarehousePage
    {
        private IReadOnlyList<PartnerOption> _carriers;

        public IQueryable<ShippingQueueItem> GetQueue() => new OrderService(Db).QueryShippingQueue();

        // Called once per row; the list is loaded once per request.
        public IReadOnlyList<PartnerOption> GetCarriers() => _carriers ?? (_carriers = new CarrierService(Db).GetOptions());

        protected void Queue_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (e.CommandName != "Ship" || !IsValid)
            {
                return;
            }

            var orderId = int.Parse((string)e.CommandArgument, CultureInfo.InvariantCulture);
            var carrierList = (DropDownList)e.Item.FindControl("CarrierList");
            var trackingBox = (TextBox)e.Item.FindControl("TrackingBox");
            var carrierId = int.Parse(carrierList.SelectedValue, CultureInfo.InvariantCulture);

            if (TryRun(() => new OrderService(Db).Ship(orderId, carrierId, trackingBox.Text, CurrentUserName)))
            {
                RedirectWithMessage("~/shipping", $"{Order.FormatNumber(orderId)} shipped with {carrierList.SelectedItem.Text}.");
            }
            else
            {
                QueueGrid.DataBind();
            }
        }
    }
}
