using System;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Customers
{
    public partial class CustomerList : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        public IQueryable<PartnerListItem> GetCustomers([QueryString("q")] string search) =>
            new CustomerService(Db).Query(search);

        public void DeleteCustomer(int id)
        {
            if (TryRun(() => new CustomerService(Db).Delete(id)))
            {
                ShowAlert("Customer deleted.", AlertKind.Success);
            }
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            var term = SearchBox.Text.Trim();
            RedirectTo("~/customers" + (term.Length > 0 ? "?q=" + HttpUtility.UrlEncode(term) : string.Empty));
        }
    }
}
