using System;
using System.Data.SqlClient;
using Warehouse.Core.Data;
using Xunit;

namespace Warehouse.Core.Tests.Integration
{
    /// <summary>
    /// Provisions a throw-away database from the real schema and seed scripts on SQL Server LocalDB
    /// (or the server named in the WMS_TEST_SQLSERVER environment variable) and drops it afterwards.
    /// When no server is reachable the integration tests are skipped instead of failing.
    /// </summary>
    public sealed class LocalDbFixture : IDisposable
    {
        public LocalDbFixture()
        {
            var server = Environment.GetEnvironmentVariable("WMS_TEST_SQLSERVER");
            ConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = string.IsNullOrWhiteSpace(server) ? @"(localdb)\MSSQLLocalDB" : server,
                InitialCatalog = "WarehouseTests_" + Guid.NewGuid().ToString("N"),
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ConnectTimeout = 60
            }.ConnectionString;

            try
            {
                DatabaseProvisioner.EnsureCreated(ConnectionString, seedDemoData: true);
                IsAvailable = true;
            }
            catch (SqlException ex)
            {
                UnavailableReason = "SQL Server is not available for integration tests: " + ex.Message;
            }
        }

        public string ConnectionString { get; }

        public bool IsAvailable { get; }

        public string UnavailableReason { get; }

        public WarehouseDbContext CreateContext() => new WarehouseDbContext(ConnectionString);

        public void Dispose()
        {
            if (IsAvailable)
            {
                DatabaseProvisioner.Drop(ConnectionString);
            }
        }
    }

    [CollectionDefinition(Name)]
    public sealed class LocalDbCollection : ICollectionFixture<LocalDbFixture>
    {
        public const string Name = "LocalDb";
    }
}
