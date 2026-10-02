using System.Globalization;
using Npgsql;

namespace FileSystem.ConsoleApp.Database;

public static class SqlExecutorExtensions
{
    // Interpolated values become SQL parameters, never SQL text.
    public static Task<IReadOnlyList<T>> QueryAsync<T>(
        this ISqlExecutor executor,
        FormattableString sql,
        Func<NpgsqlDataReader, T> map,
        CancellationToken cancellationToken = default)
    {
        var parameters = sql.GetArguments()
            .Select((value, index) => new NpgsqlParameter($"p{index}", value ?? DBNull.Value))
            .ToArray();

        var placeholders = parameters
            .Select(parameter => (object)$"@{parameter.ParameterName}")
            .ToArray();

        var commandText = string.Format(CultureInfo.InvariantCulture, sql.Format, placeholders);

        return executor.QueryAsync(commandText, parameters, map, cancellationToken);
    }
}
