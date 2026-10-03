using System.Configuration;

namespace Warehouse.Web.Infrastructure
{
    /// <summary>Typed access to the &lt;appSettings&gt; of Web.config.</summary>
    public static class AppSettings
    {
        /// <summary>Create the database from database/schema.sql on start-up when it does not exist.</summary>
        public static bool ProvisionDatabase => GetBoolean("Warehouse:ProvisionDatabase");

        /// <summary>Load database/seed.sql into a newly provisioned database.</summary>
        public static bool SeedDemoData => GetBoolean("Warehouse:SeedDemoData");

        /// <summary>Show the demo credentials on the sign-in page.</summary>
        public static bool ShowDemoCredentials => GetBoolean("Warehouse:ShowDemoCredentials");

        private static bool GetBoolean(string key) =>
            bool.TryParse(ConfigurationManager.AppSettings[key], out var value) && value;
    }
}
