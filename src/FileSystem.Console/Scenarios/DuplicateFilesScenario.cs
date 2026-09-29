using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class DuplicateFilesScenario : ScenarioBase
{
    public DuplicateFilesScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 6;
    public override string Name =>
        "Поиск файлов-дубликатов";

    protected override string SqlFileName =>
        "scenario-06-duplicate-files.sql";

    protected override string NotesFileName =>
        "scenario-06-duplicate-files.md";

    protected override async Task RunAsync()
    {
        var sql = await Resources.ReadSqlAsync(
            SqlFileName);

        try
        {
            var duplicates = await SqlExecutor.QueryAsync(
                sql,
                Array.Empty<Npgsql.NpgsqlParameter>(),
                reader => new DuplicateFileResult(
                    reader.GetString(
                        reader.GetOrdinal("name")),
                    reader.GetInt64(
                        reader.GetOrdinal("duplicate_count"))));

            if (duplicates.Count == 0)
            {
                Output.WriteWarning(
                    "Файлы-дубликаты не найдены.");

                return;
            }

            foreach (var duplicate in duplicates)
            {
                Output.Write(
                    $"{duplicate.Name} — дубликатов: {duplicate.DuplicateCount}");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Не удалось выполнить запрос: {exception.Message}");
        }
    }

    private sealed record DuplicateFileResult(
        string Name,
        long DuplicateCount);
}
