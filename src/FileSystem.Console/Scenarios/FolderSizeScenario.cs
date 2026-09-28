using Npgsql;
using NpgsqlTypes;
using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class FolderSizeScenario : ScenarioBase
{
    public FolderSizeScenario(ISqlExecutor sqlExecutor, ScenarioResourceReader resources, ConsoleInput input, ConsoleOutput output)
        : base(sqlExecutor, resources, input, output) { }

    public override int Number => 1;
    public override string Name => "Суммарный размер файлов папки";
    protected override string SqlFileName => "scenario-01-folder-size.sql";
    protected override string NotesFileName => "scenario-01-folder-size.md";

    protected override async Task RunAsync()
    {
        var folderIdText = Input.ReadText("Введите FolderId");
        if (!int.TryParse(folderIdText, out var folderId) || folderId <= 0)
        {
            Output.WriteError("FolderId должен быть положительным целым числом.");
            return;
        }

        var sql = await Resources.ReadSqlAsync(SqlFileName);
        var parameters = new[]
        {
            new NpgsqlParameter("folder_id", NpgsqlDbType.Integer) { Value = folderId }
        };

        try
        {
            var results = await SqlExecutor.QueryAsync(
                sql,
                parameters,
                reader => reader.IsDBNull(reader.GetOrdinal("total_size_bytes")) ? 0L : reader.GetInt64(reader.GetOrdinal("total_size_bytes")));

            Output.Write($"Суммарный размер файлов: {results.Single()} байт");
        }
        catch (Exception exception)
        {
            Output.WriteError($"Не удалось выполнить запрос: {exception.Message}");
        }
    }
}