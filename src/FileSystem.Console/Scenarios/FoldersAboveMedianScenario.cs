using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class FoldersAboveMedianScenario : ScenarioBase
{
    public FoldersAboveMedianScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 8;
    public override string Name =>
        "Папки выше медианы вложенных файлов";

    protected override string SqlFileName =>
        "scenario-08-folders-above-median.sql";

    protected override string NotesFileName =>
        "scenario-08-folders-above-median.md";

    protected override async Task RunAsync()
    {
        var sql = await Resources.ReadSqlAsync(
            SqlFileName);

        try
        {
            var folders = await SqlExecutor.QueryAsync(
                sql,
                Array.Empty<Npgsql.NpgsqlParameter>(),
                reader => new FolderResult(
                    reader.GetInt32(
                        reader.GetOrdinal("folder_id")),
                    reader.GetString(
                        reader.GetOrdinal("folder_name")),
                    reader.GetInt64(
                        reader.GetOrdinal("file_count"))));

            if (folders.Count == 0)
            {
                Output.WriteWarning(
                    "Папок выше медианы не найдено.");

                return;
            }

            foreach (var folder in folders)
            {
                Output.Write(
                    $"{folder.FolderName} " +
                    $"(Id: {folder.FolderId}) — " +
                    $"файлов: {folder.FileCount}");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить запрос: {exception.Message}");
        }
    }

    private sealed record FolderResult(
        int FolderId,
        string FolderName,
        long FileCount);
}
