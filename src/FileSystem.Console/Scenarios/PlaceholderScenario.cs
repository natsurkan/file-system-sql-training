using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public sealed class PlaceholderScenario : ScenarioBase
{
    private readonly int _number;
    private readonly string _name;
    private readonly string _sqlFileName;

    public PlaceholderScenario(int number, string name, string sqlFileName, ISqlExecutor sqlExecutor, ScenarioResourceReader resources, ConsoleInput input, ConsoleOutput output)
        : base(sqlExecutor, resources, input, output) { _number = number; _name = name; _sqlFileName = sqlFileName; }

    public override int Number => _number;
    public override string Name => _name;
    protected override string SqlFileName => _sqlFileName;
    protected override string NotesFileName => _sqlFileName.Replace(".sql", ".md");

    protected override Task RunAsync()
    {
        Output.WriteWarning("Не реализовано: SQL пока является заглушкой, поэтому реальный результат недоступен.");
        return Task.CompletedTask;
    }
}
