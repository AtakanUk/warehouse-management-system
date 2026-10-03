using System;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Customers
{
    public partial class CustomerEdit : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Title = RouteId.HasValue ? "Edit customer" : "New customer";

            if (!IsPostBack && RouteId.HasValue)
            {
                var customer = new CustomerService(Db).Find(RouteId.Value);
                if (customer == null)
                {
                    ShowNotFound();
                    return;
                }

                ContactForm.Details = new PartnerDetails
                {
                    Name = customer.Name,
                    ContactName = customer.ContactName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address
                };
            }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var customers = new CustomerService(Db);
            var details = ContactForm.Details;
            var saved = RouteId.HasValue
                ? TryRun(() => customers.Update(RouteId.Value, details))
                : TryRun(() => customers.Create(details));

            if (saved)
            {
                RedirectWithMessage("~/customers", $"Customer {details.Name.Trim()} saved.");
            }
        }
    }
}
