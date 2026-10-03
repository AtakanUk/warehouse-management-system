using System;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Products
{
    public partial class ProductList : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchBox.Text = Request.QueryString["q"];
            }
        }

        // Model binding: returning IQueryable lets the ListView page and sort in SQL.
        public IQueryable<ProductListItem> GetProducts([QueryString("q")] string search) =>
            new ProductService(Db).Query(search);

        public void DeleteProduct(int id)
        {
            if (TryRun(() => new ProductService(Db).Delete(id)))
            {
                ShowAlert("Product deleted.", AlertKind.Success);
            }
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            var term = SearchBox.Text.Trim();
            RedirectTo("~/products" + (term.Length > 0 ? "?q=" + HttpUtility.UrlEncode(term) : string.Empty));
        }
    }
}
