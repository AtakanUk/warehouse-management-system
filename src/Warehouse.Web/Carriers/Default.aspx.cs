using System;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Carriers
{
    public partial class CarrierList : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        public IQueryable<PartnerListItem> GetCarriers([QueryString("q")] string search) =>
            new CarrierService(Db).Query(search);

        public void DeleteCarrier(int id)
        {
            if (TryRun(() => new CarrierService(Db).Delete(id)))
            {
                ShowAlert("Carrier deleted.", AlertKind.Success);
            }
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            var term = SearchBox.Text.Trim();
            RedirectTo("~/carriers" + (term.Length > 0 ? "?q=" + HttpUtility.UrlEncode(term) : string.Empty));
        }
    }
}
