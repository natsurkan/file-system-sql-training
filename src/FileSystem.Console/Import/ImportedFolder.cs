namespace FileSystem.ConsoleApp.Import;

public sealed class ImportedFolder
{
    public ImportedFolder(
        string name,
        IReadOnlyList<ImportedFile> files,
        IReadOnlyList<ImportedFolder> childFolders)
    {
        Name = name;
        Files = files;
        ChildFolders = childFolders;
    }

    public string Name { get; }
    public IReadOnlyList<ImportedFile> Files { get; }
    public IReadOnlyList<ImportedFolder> ChildFolders { get; }
}
