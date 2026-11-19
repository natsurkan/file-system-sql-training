using Npgsql;

namespace FileSystem.ConsoleApp.Database;
public sealed class NpgsqlSqlExecutor : ISqlExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    public NpgsqlSqlExecutor(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;
    public async Task<IReadOnlyList<T>> QueryCursorAsync<T>(string sql, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        // The year list and totals must see the same data snapshot.
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
        await using var openCommand = new NpgsqlCommand(sql, connection, transaction);
        var cursorName = (string?)await openCommand.ExecuteScalarAsync(cancellationToken)
            ?? throw new InvalidOperationException("Запрос не вернул имя курсора.");
        using var builder = new NpgsqlCommandBuilder();
        await using var fetchCommand = new NpgsqlCommand(
            "FETCH ALL FROM " + builder.QuoteIdentifier(cursorName), connection, transaction);
        var result = new List<T>();
        await using (var reader = await fetchCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken)) result.Add(map(reader));
        }
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
    public async Task ExecuteAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, IEnumerable<NpgsqlParameter> parameters, Func<NpgsqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<T>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(map(reader));
        return result;
    }
}
