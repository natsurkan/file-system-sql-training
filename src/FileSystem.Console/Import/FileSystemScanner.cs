namespace FileSystem.ConsoleApp.Import;

public sealed class FileSystemScanner
{
    public ImportedFolder Scan(string physicalFolderPath)
    {
        var fullPath = Path.GetFullPath(physicalFolderPath);

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(
                $"Физическая папка не найдена: {fullPath}");
        }

        return ScanFolder(new DirectoryInfo(fullPath));
    }

    private static ImportedFolder ScanFolder(DirectoryInfo physicalFolder)
    {
        var files = physicalFolder
            .EnumerateFiles()
            .Select(file =>
            {
                var content = File.ReadAllBytes(file.FullName);

                return new ImportedFile(
                    file.Name,
                    content,
                    file.Length);
            })
            .ToList();

        var childFolders = physicalFolder
            .EnumerateDirectories()
            .Select(ScanFolder)
            .ToList();

        return new ImportedFolder(
            physicalFolder.Name,
            files,
            childFolders);
    }
}
