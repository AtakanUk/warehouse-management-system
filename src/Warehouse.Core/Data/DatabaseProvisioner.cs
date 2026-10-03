using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Warehouse.Core.Data
{
    /// <summary>
    /// Creates the database from the embedded <c>database/schema.sql</c> (and optionally
    /// <c>database/seed.sql</c>) when it does not exist yet, so a fresh clone runs with one key press.
    /// </summary>
    public static class DatabaseProvisioner
    {
        private static readonly Regex BatchSeparator =
            new Regex(@"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <returns><c>true</c> when the database was created by this call.</returns>
        public static bool EnsureCreated(string connectionString, bool seedDemoData)
        {
            var target = new SqlConnectionStringBuilder(connectionString);
            var databaseName = target.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("The connection string must specify an Initial Catalog.");
            }

            var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
            using (var connection = new SqlConnection(master.ConnectionString))
            {
                connection.Open();
                if (DatabaseExists(connection, databaseName))
                {
                    return false;
                }

                using (var create = new SqlCommand($"CREATE DATABASE {QuoteName(databaseName)};", connection))
                {
                    create.ExecuteNonQuery();
                }
            }

            try
            {
                Execute(target.ConnectionString, ReadScript("schema.sql"));
                if (seedDemoData)
                {
                    Execute(target.ConnectionString, ReadScript("seed.sql"));
                }
            }
            catch
            {
                // Never leave a half-initialised database behind; the next start retries cleanly.
                Drop(connectionString);
                throw;
            }

            return true;
        }

        /// <summary>Drops the database referenced by the connection string if it exists.</summary>
        public static void Drop(string connectionString)
        {
            var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };

            SqlConnection.ClearAllPools();
            using (var connection = new SqlConnection(master.ConnectionString))
            {
                connection.Open();
                if (!DatabaseExists(connection, databaseName))
                {
                    return;
                }

                var name = QuoteName(databaseName);
                using (var drop = new SqlCommand($"ALTER DATABASE {name} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {name};", connection))
                {
                    drop.ExecuteNonQuery();
                }
            }
        }

        internal static string ReadScript(string fileName)
        {
            var resourceName = "Warehouse.Core.Data.Scripts." + fileName;
            using (var stream = typeof(DatabaseProvisioner).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Embedded script '{resourceName}' was not found.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static IEnumerable<string> SplitBatches(string script) =>
            BatchSeparator.Split(script).Select(batch => batch.Trim()).Where(batch => batch.Length > 0);

        private static void Execute(string connectionString, string script)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var batch in SplitBatches(script))
                    {
                        using (var command = new SqlCommand(batch, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private static bool DatabaseExists(SqlConnection masterConnection, string databaseName)
        {
            using (var command = new SqlCommand("SELECT DB_ID(@name);", masterConnection))
            {
                command.Parameters.AddWithValue("@name", databaseName);
                var databaseId = command.ExecuteScalar();
                return databaseId != null && databaseId != DBNull.Value;
            }
        }

        private static string QuoteName(string name) => "[" + name.Replace("]", "]]") + "]";
    }
}
