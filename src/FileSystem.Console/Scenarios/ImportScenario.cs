using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Import;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;
using Npgsql;
using NpgsqlTypes;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class ImportScenario : ScenarioBase
{
    private readonly FileSystemScanner _scanner;
    private readonly DatabaseImporter _databaseImporter;

    public ImportScenario(
        ISqlExecutor sqlExecutor,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output,
        FileSystemScanner scanner,
        DatabaseImporter databaseImporter)
        : base(sqlExecutor, resources, input, output)
    {
        _scanner = scanner;
        _databaseImporter = databaseImporter;
    }

    public override int Number => 3;
    public override string Name => "Импорт физической папки через ADO.NET";
    protected override string SqlFileName => "scenario-03-import.sql";
    protected override string NotesFileName => "scenario-03-import.md";

    protected override async Task RunAsync()
    {
        var sampleFolderPath = FindSampleFolder();

        Output.Write($"Сканируем учебную папку: {sampleFolderPath}");

        try
        {
            var physicalHierarchy =
                _scanner.Scan(sampleFolderPath);

            Output.Write("Физическая структура:");
            PrintPhysicalHierarchy(
                physicalHierarchy,
                "    ");

            Output.Write(
                "Передаём найденную иерархию в DatabaseImporter.");

            var rootFolderId =
                await _databaseImporter.ImportAsync(
                    physicalHierarchy);

            Output.Write("Иерархия сохранена в базе.");
            Output.Write(
                "Читаем и восстанавливаем иерархию из базы.");

            var databaseHierarchy =
                await ReadDatabaseHierarchyAsync(
                    rootFolderId);

            Output.Write("Структура, прочитанная из БД:");
            PrintDatabaseHierarchy(
                databaseHierarchy,
                "    ");
        }
        catch (Exception exception)
        {
            Output.WriteError(
                $"Импорт не выполнен: {exception.Message}");
        }
    }

    private async Task<DatabaseFolder> ReadDatabaseHierarchyAsync(
        int rootFolderId)
    {
        var sql = await Resources.ReadSqlAsync(
            "scenario-03-read-hierarchy.sql");

        var parameters = new[]
        {
            new NpgsqlParameter(
                "root_folder_id",
                NpgsqlDbType.Integer)
            {
                Value = rootFolderId
            }
        };

        var rows = await SqlExecutor.QueryAsync(
            sql,
            parameters,
            reader => new DatabaseRow(
                reader.GetInt32(
                    reader.GetOrdinal("folder_id")),
                reader.GetString(
                    reader.GetOrdinal("folder_name")),
                reader.IsDBNull(
                    reader.GetOrdinal("parent_folder_id"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("parent_folder_id")),
                reader.IsDBNull(
                    reader.GetOrdinal("file_name"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("file_name")),
                reader.IsDBNull(
                    reader.GetOrdinal("size_bytes"))
                    ? null
                    : reader.GetInt64(
                        reader.GetOrdinal("size_bytes"))));

        var folders = rows
            .GroupBy(row => row.FolderId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var firstRow = group.First();
                    var folder = new DatabaseFolder(
                        firstRow.FolderId,
                        firstRow.FolderName,
                        firstRow.ParentFolderId);

                    foreach (var row in group)
                    {
                        if (row.FileName is not null)
                        {
                            folder.Files.Add(
                                new DatabaseFile(
                                    row.FileName,
                                    row.SizeBytes!.Value));
                        }
                    }

                    return folder;
                });

        foreach (var folder in folders.Values)
        {
            if (folder.ParentFolderId is not null)
            {
                folders[folder.ParentFolderId.Value]
                    .ChildFolders
                    .Add(folder);
            }
        }

        return folders[rootFolderId];
    }

    private void PrintPhysicalHierarchy(
        ImportedFolder folder,
        string indentation)
    {
        Output.Write($"{indentation}папка: {folder.Name}");

        foreach (var file in folder.Files)
        {
            Output.Write(
                $"{indentation}    файл: {file.Name} ({file.SizeBytes} байт)");
        }

        foreach (var childFolder in folder.ChildFolders)
        {
            PrintPhysicalHierarchy(
                childFolder,
                indentation + "    ");
        }
    }

    private void PrintDatabaseHierarchy(
        DatabaseFolder folder,
        string indentation)
    {
        Output.Write(
            $"{indentation}папка: {folder.Name} (Id: {folder.Id})");

        foreach (var file in folder.Files)
        {
            Output.Write(
                $"{indentation}    файл: {file.Name} ({file.SizeBytes} байт)");
        }

        foreach (var childFolder in folder.ChildFolders)
        {
            PrintDatabaseHierarchy(
                childFolder,
                indentation + "    ");
        }
    }

    private static string FindSampleFolder()
    {
        var current = AppContext.BaseDirectory;

        while (current is not null)
        {
            var sampleFolderPath = Path.Combine(
                current,
                "sample-data",
                "import-source");

            if (Directory.Exists(sampleFolderPath))
            {
                return sampleFolderPath;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new DirectoryNotFoundException(
            "В проекте не найдена папка sample-data/import-source.");
    }

    private sealed record DatabaseRow(
        int FolderId,
        string FolderName,
        int? ParentFolderId,
        string? FileName,
        long? SizeBytes);

    private sealed class DatabaseFolder
    {
        public DatabaseFolder(
            int id,
            string name,
            int? parentFolderId)
        {
            Id = id;
            Name = name;
            ParentFolderId = parentFolderId;
        }

        public int Id { get; }
        public string Name { get; }
        public int? ParentFolderId { get; }
        public List<DatabaseFile> Files { get; } = new();
        public List<DatabaseFolder> ChildFolders { get; } = new();
    }

    private sealed record DatabaseFile(
        string Name,
        long SizeBytes);
}
