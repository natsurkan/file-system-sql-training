using Npgsql;
using NpgsqlTypes;
using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class FilePathScenario : ScenarioBase
{
    public FilePathScenario(ISqlExecutor sqlExecutor, ScenarioResourceReader resources, ConsoleInput input, ConsoleOutput output)
        : base(sqlExecutor, resources, input, output) { }

    public override int Number => 2;
    public override string Name => "Полный путь родительских папок файла";
    protected override string SqlFileName => "scenario-02-file-path.sql";
    protected override string NotesFileName => "scenario-02-file-path.md";

    protected override async Task RunAsync()
    {
        var fileIdText = Input.ReadText("Введите FileId");
        if (!int.TryParse(fileIdText, out var fileId) || fileId <= 0)
        {
            Output.WriteError("FileId должен быть положительным целым числом.");
            return;
        }

        var sql = await Resources.ReadSqlAsync(SqlFileName);
        var parameters = new[]
        {
            new NpgsqlParameter("file_id", NpgsqlDbType.Integer) { Value = fileId }
        };

        try
        {
            var paths = await SqlExecutor.QueryAsync(
                sql,
                parameters,
                reader => reader.GetString(reader.GetOrdinal("full_path")));

            if (paths.Count == 0)
            {
                Output.WriteWarning("Файл не найден или путь не построен.");
                return;
            }

            foreach (var path in paths) Output.Write(path);
        }
        catch (Exception exception)
        {
            Output.WriteError($"Не удалось выполнить запрос: {exception.Message}");
        }
    }
}