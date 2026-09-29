using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;
using Npgsql;
using NpgsqlTypes;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class FileSizeQuartilesScenario : ScenarioBase
{
    public FileSizeQuartilesScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 4;
    public override string Name =>
        "Квартили размера файлов";

    protected override string SqlFileName =>
        "scenario-04-file-size-quartiles.sql";

    protected override string NotesFileName =>
        "scenario-04-file-size-quartiles.md";

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
                reader => new QuartileResult(
                    ReadNullableDouble(
                        reader,
                        "first_quartile"),
                    ReadNullableDouble(
                        reader,
                        "second_quartile"),
                    ReadNullableDouble(
                        reader,
                        "third_quartile")));

            var result = results.Single();

            if (result.FirstQuartile is null)
            {
                Output.WriteWarning(
                    "В выбранной папке и её вложенных папках нет файлов.");

                return;
            }

            Output.Write(
                $"Q1: {result.FirstQuartile} байт");

            Output.Write(
                $"Q2: {result.SecondQuartile} байт");

            Output.Write(
                $"Q3: {result.ThirdQuartile} байт");
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

    private sealed record QuartileResult(
        double? FirstQuartile,
        double? SecondQuartile,
        double? ThirdQuartile);
}
