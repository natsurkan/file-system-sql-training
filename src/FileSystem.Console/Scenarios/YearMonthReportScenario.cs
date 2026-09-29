using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class YearMonthReportScenario : ScenarioBase
{
    public YearMonthReportScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 7;
    public override string Name =>
        "Размеры по годам и месяцам";

    protected override string SqlFileName =>
        "scenario-07-year-month-report.sql";

    protected override string NotesFileName =>
        "scenario-07-year-month-report.md";

    protected override async Task RunAsync()
    {
        var sql = await Resources.ReadSqlAsync(
            SqlFileName);

        try
        {
            var rows = await SqlExecutor.QueryAsync(
                sql,
                Array.Empty<Npgsql.NpgsqlParameter>(),
                reader => new YearMonthRow(
                    reader.GetInt32(
                        reader.GetOrdinal("month")),
                    reader.GetInt64(
                        reader.GetOrdinal("2025")),
                    reader.GetInt64(
                        reader.GetOrdinal("2026")),
                    reader.GetInt64(
                        reader.GetOrdinal("2027"))));

            if (rows.Count == 0)
            {
                Output.WriteWarning(
                    "Файлы для отчёта не найдены.");

                return;
            }

            Output.Write(
                "Месяц | 2025 | 2026 | 2027");

            foreach (var row in rows)
            {
                Output.Write(
                    $"{row.Month,5} | {row.Year2025,4} | {row.Year2026,4} | {row.Year2027,4}");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить запрос: {exception.Message}");
        }
    }

    private sealed record YearMonthRow(
        int Month,
        long Year2025,
        long Year2026,
        long Year2027);
}
