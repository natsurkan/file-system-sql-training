using Npgsql;

namespace FileSystem.ConsoleApp.Database;
public interface IDbConnectionFactory { NpgsqlConnection CreateConnection(); }
