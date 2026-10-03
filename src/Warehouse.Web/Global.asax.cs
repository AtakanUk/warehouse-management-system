using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Routing;
using Warehouse.Core.Data;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            if (AppSettings.ProvisionDatabase)
            {
                var connectionString = ConfigurationManager.ConnectionStrings[WarehouseDbContext.ConnectionStringName].ConnectionString;
                if (DatabaseProvisioner.EnsureCreated(connectionString, AppSettings.SeedDemoData))
                {
                    Trace.TraceInformation("Created the warehouse database from database/schema.sql.");
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception is HttpException http && http.GetHttpCode() < 500)
            {
                return;
            }

            Trace.TraceError("Unhandled exception for {0}: {1}", Context?.Request.RawUrl, exception);
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            // Do not advertise the server software (X-Powered-By and X-AspNet-Version are removed in Web.config).
            if (HttpRuntime.UsingIntegratedPipeline)
            {
                Response.Headers.Remove("Server");
            }
        }
    }
}
