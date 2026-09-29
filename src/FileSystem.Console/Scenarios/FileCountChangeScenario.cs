using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;
using Npgsql;
using NpgsqlTypes;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class FileCountChangeScenario : ScenarioBase
{
    public FileCountChangeScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 5;
    public override string Name =>
        "Сравнение файлов за сегодня и вчера";

    protected override string SqlFileName =>
        "scenario-05-today-vs-yesterday.sql";

    protected override string NotesFileName =>
        "scenario-05-today-vs-yesterday.md";

    protected override async Task RunAsync()
    {
        var folderIdText = Input.ReadText(
            "Введите FolderId");

        if (!int.TryParse(folderIdText, out var folderId)
            || folderId <= 0)
        {
            Output.WriteError(
                "FolderId должен быть положительным целым числом.");

            return;
        }

        var sql = await Resources.ReadSqlAsync(
            SqlFileName);

        var parameters = new[]
        {
            new NpgsqlParameter(
                "folder_id",
                NpgsqlDbType.Integer)
            {
                Value = folderId
            }
        };

        try
        {
            var results = await SqlExecutor.QueryAsync(
                sql,
                parameters,
                reader => new FileCountChangeResult(
                    reader.GetInt64(
                        reader.GetOrdinal("today_count")),
                    reader.GetInt64(
                        reader.GetOrdinal("yesterday_count")),
                    reader.GetInt64(
                        reader.GetOrdinal("difference")),
                    ReadNullableDouble(
                        reader,
                        "percentage_change")));

            var result = results.Single();

            Output.Write(
                $"Сегодня создано файлов: {result.TodayCount}");

            Output.Write(
                $"Вчера создано файлов: {result.YesterdayCount}");

            Output.Write(
                $"Разница: {result.Difference}");

            if (result.PercentageChange is null)
            {
                Output.WriteWarning(
                    "Процент не рассчитан: вчера файлов не было.");
            }
            else
            {
                Output.Write(
                    $"Изменение относительно вчера: {result.PercentageChange}%");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить запрос: {exception.Message}");
        }
    }

    private static double? ReadNullableDouble(
        NpgsqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetDouble(ordinal);
    }

    private sealed record FileCountChangeResult(
        long TodayCount,
        long YesterdayCount,
        long Difference,
        double? PercentageChange);
}
