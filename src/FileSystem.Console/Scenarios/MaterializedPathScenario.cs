using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class MaterializedPathScenario : ScenarioBase
{
    public MaterializedPathScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output)
        : base(sqlExecutor, resources, input, output)
    {
    }

    public override int Number => 9;
    public override string Name =>
        "Materialized Path, индекс и backfill";

    protected override string SqlFileName =>
        "scenario-09-backfill-folders.sql";

    protected override string NotesFileName =>
        "scenario-09-materialized-path.md";

    protected override async Task RunAsync()
    {
        try
        {
            await ExecuteStepAsync(
                "database/migrations/003_add_materialized_path.sql",
                "Добавляем колонки Materialized Path");

            await ExecuteStepAsync(
                "scenario-09-backfill-folders.sql",
                "Заполняем пути папок");

            await ExecuteStepAsync(
                "scenario-09-backfill-files.sql",
                "Заполняем пути файлов");

            await ExecuteStepAsync(
                "scenario-09-create-indexes.sql",
                "Создаём индексы");

            var rows = await ReadPathsAsync();

            Output.Write("Пути, прочитанные из базы:");

            foreach (var row in rows)
            {
                Output.Write(
                    $"{row.ObjectType}: " +
                    $"{row.ObjectName} — " +
                    $"{row.MaterializedPath}");
            }
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Materialized Path не обработан: {exception.Message}");
        }
    }

    private async Task ExecuteStepAsync(
        string fileName,
        string description)
    {
        Output.Write(description);

        var sql = fileName.StartsWith("database/")
            ? await Resources.ReadMigrationAsync(
                "003_add_materialized_path.sql")
            : await Resources.ReadSqlAsync(fileName);

        await SqlExecutor.ExecuteAsync(
            sql,
            Array.Empty<Npgsql.NpgsqlParameter>());
    }

    private async Task<IReadOnlyList<PathRow>> ReadPathsAsync()
    {
        var sql = await Resources.ReadSqlAsync(
            "scenario-09-show-materialized-path.sql");

        return await SqlExecutor.QueryAsync(
            sql,
            Array.Empty<Npgsql.NpgsqlParameter>(),
            reader => new PathRow(
                reader.GetString(
                    reader.GetOrdinal("object_type")),
                reader.GetString(
                    reader.GetOrdinal("object_name")),
                reader.GetString(
                    reader.GetOrdinal("materialized_path"))));
    }

    private sealed record PathRow(
        string ObjectType,
        string ObjectName,
        string MaterializedPath);
}
