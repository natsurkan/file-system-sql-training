using Npgsql;

namespace FileSystem.ConsoleApp.Database;
public interface ISqlExecutor
{
    Task ExecuteAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default);
}
