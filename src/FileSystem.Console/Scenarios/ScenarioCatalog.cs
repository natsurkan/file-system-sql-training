using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Import;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public static class ScenarioCatalog
{
    public static IReadOnlyList<IScenario> CreateDefault(
        ISqlExecutor executor,
        IDbConnectionFactory connectionFactory,
        ScenarioResourceReader resources,
        ConsoleInput input,
        ConsoleOutput output) => new IScenario[]
    {
        new FolderSizeScenario(executor, resources, input, output),
        new FilePathScenario(executor, resources, input, output),
        new ImportScenario(
            executor,
            resources,
            input,
            output,
            new FileSystemScanner(),
            new DatabaseImporter(connectionFactory, resources)),
        new FileSizeQuartilesScenario(executor, resources, input, output),
        new FileCountChangeScenario(executor, resources, input, output),
        new DuplicateFilesScenario(executor, resources, input, output),
        new YearMonthReportScenario(executor, resources, input, output),
        new FoldersAboveMedianScenario(executor, resources, input, output),
        new MaterializedPathScenario(executor, resources, input, output),
        new PartitioningScenario(executor, resources, input, output)
    };
}







