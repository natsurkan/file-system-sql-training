using Npgsql;

namespace FileSystem.ConsoleApp.Database;
public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
        ?? "Host=127.0.0.1;Port=15432;Database=filesystem_training;Username=postgres;Password=postgres";
    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
