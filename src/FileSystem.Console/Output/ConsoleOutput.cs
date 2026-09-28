using FileSystem.ConsoleApp.Scenarios;

namespace FileSystem.ConsoleApp.Output;

public sealed class ConsoleOutput
{
    public void ShowMenu(IReadOnlyList<IScenario> scenarios)
    {
        Console.Clear();
        WriteTitle("SQL / ADO.NET Training");
        foreach (var scenario in scenarios) Console.WriteLine($"{scenario.Number}. {scenario.Name}");
        Console.WriteLine("0. Выход\n");
    }
    public void WriteTitle(string text) => Console.WriteLine($"\n=== {text} ===\n");
    public void Write(string text) => Console.WriteLine(text);
    public void WriteWarning(string text) => Console.WriteLine($"[ЗАГЛУШКА] {text}");
    public void WriteError(string text) => Console.WriteLine($"[ОШИБКА] {text}");
    public void WriteBlock(string text) => Console.WriteLine($"\n{text}");
}
