using System;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Carriers
{
    public partial class CarrierEdit : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Title = RouteId.HasValue ? "Edit carrier" : "New carrier";

            if (!IsPostBack && RouteId.HasValue)
            {
                var carrier = new CarrierService(Db).Find(RouteId.Value);
                if (carrier == null)
                {
                    ShowNotFound();
                    return;
                }

                ContactForm.Details = new PartnerDetails
                {
                    Name = carrier.Name,
                    ContactName = carrier.ContactName,
                    Email = carrier.Email,
                    Phone = carrier.Phone,
                    Address = carrier.Address
                };
            }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var carriers = new CarrierService(Db);
            var details = ContactForm.Details;
            var saved = RouteId.HasValue
                ? TryRun(() => carriers.Update(RouteId.Value, details))
                : TryRun(() => carriers.Create(details));

            if (saved)
            {
                RedirectWithMessage("~/carriers", $"Carrier {details.Name.Trim()} saved.");
            }
        }
    }
}
