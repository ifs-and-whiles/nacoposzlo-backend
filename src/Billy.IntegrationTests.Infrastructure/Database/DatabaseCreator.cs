using System.Threading.Tasks;
using Billy.Infrastructure.Configs;
using Npgsql;
using Serilog;

namespace Billy.IntegrationTests.Infrastructure.Database
{
    class DatabaseCreator
    {
        private static readonly ILogger Logger = Log.ForContext<DatabaseCreator>();

        public static async Task CreateIfNotExists(DatabaseConfig config)
        {
            var builder = new NpgsqlConnectionStringBuilder(config.ConnectionString);
            var database = builder.Database;

            builder.Database = "postgres";
            var postgresConnectionString = builder.ConnectionString;

            using (var connection = new NpgsqlConnection(postgresConnectionString))
            {
                await connection.OpenAsync();

                var exists =
                    await new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{database}'", connection)
                        .ExecuteScalarAsync();

                if (exists == null)
                {
                    Logger.Information("Creating database {database}", database);
                    await new NpgsqlCommand($"CREATE DATABASE {database}", connection).ExecuteNonQueryAsync();
                }
            }
        }
    }
}
