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
        try
        {
            var migration = await Resources.ReadMigrationAsync("005_add_year_month_report.sql");
            await SqlExecutor.ExecuteAsync(migration, Array.Empty<Npgsql.NpgsqlParameter>());
            var sql = await Resources.ReadSqlAsync(SqlFileName);
            string[] columns = Array.Empty<string>();
            var rows = await SqlExecutor.QueryCursorAsync(
                sql,
                reader =>
                {
                    if (columns.Length == 0)
                        columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                    var values = new string[reader.FieldCount];
                    for (var index = 0; index < reader.FieldCount; index++)
                        values[index] = reader.GetValue(index).ToString() ?? string.Empty;
                    return values;
                });

            if (rows.Count == 0)
            {
                Output.WriteWarning(
                    "Файлы для отчёта не найдены.");

                return;
            }

            columns[0] = "Месяц";
            var widths = columns.Select(column => column.Length).ToArray();
            foreach (var row in rows)
            {
                for (var index = 0; index < widths.Length; index++)
                    widths[index] = Math.Max(widths[index], row[index].Length);
            }

            var borderSegments = widths.Select(width => new string('─', width + 2)).ToArray();
            Output.WriteTitle("Размер файлов по месяцам и годам · байты");
            Output.Write("┌" + string.Join("┬", borderSegments) + "┐");
            Output.Write(FormatRow(columns, widths));
            Output.Write("├" + string.Join("┼", borderSegments) + "┤");

            foreach (var row in rows)
            {
                Output.Write(FormatRow(row, widths));
            }
            Output.Write("└" + string.Join("┴", borderSegments) + "┘");
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить запрос: {exception.Message}");
        }
    }

    private static string FormatRow(string[] values, int[] widths)
    {
        var cells = new string[values.Length];
        cells[0] = values[0].PadRight(widths[0]);
        for (var index = 1; index < values.Length; index++)
            cells[index] = values[index].PadLeft(widths[index]);
        return "│ " + string.Join(" │ ", cells) + " │";
    }
}
