using System;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web
{
    public partial class NotFoundPage : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
