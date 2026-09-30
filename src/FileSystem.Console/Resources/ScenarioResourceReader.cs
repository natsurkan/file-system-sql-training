namespace FileSystem.ConsoleApp.Resources;

public sealed class ScenarioResourceReader
{
    public Task<string> ReadSqlAsync(string fileName) => ReadResourceAsync("database", "queries", fileName);
    public Task<string> ReadNotesAsync(string fileName) => ReadResourceAsync("docs", "scenarios", fileName);
    public Task<string> ReadMigrationAsync(string fileName) => ReadResourceAsync("database", "migrations", fileName);

    private static Task<string> ReadResourceAsync(string rootFolder, string subFolder, string fileName)
    {
        var current = AppContext.BaseDirectory;
        while (current is not null)
        {
            var candidate = Path.Combine(current, rootFolder, subFolder, fileName);
            if (File.Exists(candidate)) return File.ReadAllTextAsync(candidate);
            current = Directory.GetParent(current)?.FullName;
        }

        throw new FileNotFoundException($"Не найден ресурс: {rootFolder}/{subFolder}/{fileName}");
    }
}
