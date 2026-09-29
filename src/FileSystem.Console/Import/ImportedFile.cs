namespace FileSystem.ConsoleApp.Import;

public sealed class ImportedFile
{
    public ImportedFile(string name, byte[] content, long sizeBytes)
    {
        Name = name;
        Content = content;
        SizeBytes = sizeBytes;
    }

    public string Name { get; }
    public byte[] Content { get; }
    public long SizeBytes { get; }
}
