using System.Web.Routing;

namespace Warehouse.Web
{
    /// <summary>Extensionless, resource-oriented URLs for every screen.</summary>
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // URLs such as /products share their name with the folder holding the page; without
            // this, IIS treats them as directories and answers with a 301 to /products/.
            // Static files are unaffected because no route pattern matches them.
            routes.RouteExistingFiles = true;

            routes.MapPageRoute("Dashboard", string.Empty, "~/Default.aspx");

            routes.MapPageRoute("Login", "account/login", "~/Account/Login.aspx");
            routes.MapPageRoute("Register", "account/register", "~/Account/Register.aspx");

            routes.MapPageRoute("Products", "products", "~/Products/Default.aspx");
            routes.MapPageRoute("ProductNew", "products/new", "~/Products/Edit.aspx");
            MapWithId(routes, "ProductEdit", "products/{id}/edit", "~/Products/Edit.aspx");

            routes.MapPageRoute("Inventory", "inventory", "~/Inventory/Default.aspx");
            routes.MapPageRoute("Receive", "inventory/receive", "~/Inventory/Receive.aspx");
            routes.MapPageRoute("Movements", "inventory/movements", "~/Inventory/Movements.aspx");

            routes.MapPageRoute("Orders", "orders", "~/Orders/Default.aspx");
            routes.MapPageRoute("OrderNew", "orders/new", "~/Orders/New.aspx");
            MapWithId(routes, "OrderDetails", "orders/{id}", "~/Orders/Details.aspx");

            routes.MapPageRoute("Picking", "picking", "~/Picking/Default.aspx");
            routes.MapPageRoute("Shipping", "shipping", "~/Shipping/Default.aspx");

            routes.MapPageRoute("Customers", "customers", "~/Customers/Default.aspx");
            routes.MapPageRoute("CustomerNew", "customers/new", "~/Customers/Edit.aspx");
            MapWithId(routes, "CustomerEdit", "customers/{id}/edit", "~/Customers/Edit.aspx");

            routes.MapPageRoute("Carriers", "carriers", "~/Carriers/Default.aspx");
            routes.MapPageRoute("CarrierNew", "carriers/new", "~/Carriers/Edit.aspx");
            MapWithId(routes, "CarrierEdit", "carriers/{id}/edit", "~/Carriers/Edit.aspx");

            routes.MapPageRoute("Error", "error", "~/Error.aspx");
            routes.MapPageRoute("NotFound", "not-found", "~/NotFound.aspx");
        }

        private static void MapWithId(RouteCollection routes, string name, string url, string physicalFile)
        {
            routes.MapPageRoute(name, url, physicalFile, checkPhysicalUrlAccess: true,
                defaults: null, constraints: new RouteValueDictionary { { "id", @"\d{1,9}" } });
        }
    }
}
