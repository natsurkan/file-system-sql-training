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
        new PlaceholderScenario(5, "Сравнение файлов за сегодня и вчера", "scenario-05-today-vs-yesterday.sql", executor, resources, input, output),
        new PlaceholderScenario(6, "Поиск файлов-дубликатов", "scenario-06-duplicate-files.sql", executor, resources, input, output),
        new PlaceholderScenario(7, "Размеры по годам и месяцам", "scenario-07-year-month-report.sql", executor, resources, input, output),
        new PlaceholderScenario(8, "Папки выше медианы вложенных файлов", "scenario-08-folders-above-median.sql", executor, resources, input, output),
        new PlaceholderScenario(9, "Materialized Path, индекс и backfill", "scenario-09-materialized-path.sql", executor, resources, input, output),
        new PlaceholderScenario(10, "Партиционирование", "scenario-10-partitioning.sql", executor, resources, input, output)
    };
}

