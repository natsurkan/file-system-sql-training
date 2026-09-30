using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;
using Npgsql;
using NpgsqlTypes;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class PartitioningScenario : ScenarioBase
{
    public PartitioningScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 10;
    public override string Name =>
        "Партиционирование файлов по году создания";

    protected override string SqlFileName =>
        "scenario-10-partitioning.sql";

    protected override string NotesFileName =>
        "scenario-10-partitioning.md";

    protected override async Task RunAsync()
    {
        try
        {
            if (!await IsFilesPartitionedAsync())
            {
                Output.Write("Переносим Files в партиции по годам.");

                var migration = await Resources.ReadMigrationAsync(
                    "004_partition_files_by_created_at.sql");

                await SqlExecutor.ExecuteAsync(
                    migration,
                    Array.Empty<NpgsqlParameter>());
            }

            var yearText = Input.ReadText("Введите год для проверки, например 2026");

            if (!int.TryParse(yearText, out var year)
                || year is < 1 or > 9999)
            {
                Output.WriteError("Год должен быть целым числом от 1 до 9999.");
                return;
            }

            var rows = await ReadPartitionRowsAsync(year);

            if (rows.Count == 0)
            {
                Output.Write("За этот год файлов нет.");
                return;
            }

            Output.Write("Строки хранятся в партициях:");

            foreach (var row in rows)
            {
                Output.Write(
                    $"{row.PartitionName}: файлов {row.FileCount}, " +
                    $"суммарный размер {row.TotalSizeBytes} байт.");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить партиционирование: {exception.Message}");
        }
    }

    private async Task<bool> IsFilesPartitionedAsync()
    {
        const string sql = """
            SELECT relkind = 'p' AS is_partitioned
            FROM pg_class
            WHERE oid = 'files'::regclass;
            """;

        var result = await SqlExecutor.QueryAsync(
            sql,
            Array.Empty<NpgsqlParameter>(),
            reader => reader.GetBoolean(reader.GetOrdinal("is_partitioned")));

        return result.Single();
    }

    private async Task<IReadOnlyList<PartitionRow>> ReadPartitionRowsAsync(int year)
    {
        var sql = await Resources.ReadSqlAsync(SqlFileName);
        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddYears(1);

        return await SqlExecutor.QueryAsync(
            sql,
            new[]
            {
                new NpgsqlParameter("period_start", NpgsqlDbType.TimestampTz) { Value = start },
                new NpgsqlParameter("period_end", NpgsqlDbType.TimestampTz) { Value = end }
            },
            reader => new PartitionRow(
                reader.GetString(reader.GetOrdinal("physical_partition")),
                reader.GetInt64(reader.GetOrdinal("file_count")),
                reader.GetInt64(reader.GetOrdinal("total_size_bytes"))));
    }

    private sealed record PartitionRow(
        string PartitionName,
        long FileCount,
        long TotalSizeBytes);
}
