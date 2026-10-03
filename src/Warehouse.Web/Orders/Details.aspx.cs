using System;
using System.Collections.Generic;
using System.Globalization;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Orders
{
    public partial class OrderDetails : WarehousePage
    {
        protected Order Order { get; private set; }

        private OrderService Orders => new OrderService(Db);

        protected void Page_Load(object sender, EventArgs e)
        {
            Order = RouteId.HasValue ? Orders.Find(RouteId.Value) : null;
            if (Order == null)
            {
                ShowNotFound();
                return;
            }

            if (!IsPostBack && Order.CanShip)
            {
                CarrierList.DataBind();
            }
        }

        public IReadOnlyList<PartnerOption> GetCarriers() => new CarrierService(Db).GetOptions();

        protected void Pick_Click(object sender, EventArgs e) =>
            Execute(() => Orders.Pick(Order.Id, CurrentUserName),
                $"{Order.Number} picked. Its stock is reserved and the order is ready to ship.");

        protected void Ship_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var carrierId = int.Parse(CarrierList.SelectedValue, CultureInfo.InvariantCulture);
                Execute(() => Orders.Ship(Order.Id, carrierId, TrackingBox.Text, CurrentUserName), $"{Order.Number} shipped.");
            }
        }

        protected void Cancel_Click(object sender, EventArgs e) =>
            Execute(() => Orders.Cancel(Order.Id, CurrentUserName), $"{Order.Number} cancelled.", AlertKind.Info);

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Title = Order.Number;
            PickPanel.Visible = Order.CanPick;
            ShipPanel.Visible = Order.CanShip;
            CancelPanel.Visible = Order.CanCancel;
            DonePanel.Visible = !Order.CanPick && !Order.CanShip;
            CancelledStep.Visible = Order.CancelledAtUtc.HasValue;

            LineRepeater.DataSource = Order.Lines;
            LineRepeater.DataBind();
        }

        private void Execute(Action command, string successMessage, AlertKind kind = AlertKind.Success)
        {
            if (TryRun(command))
            {
                RedirectWithMessage(OrderUrl(Order.Id), successMessage, kind);
            }
            else
            {
                Order = Orders.Find(Order.Id); // reload the stored state
            }
        }
    }
}
