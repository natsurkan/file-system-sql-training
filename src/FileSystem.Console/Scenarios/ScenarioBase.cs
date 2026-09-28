using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;

namespace FileSystem.ConsoleApp.Scenarios;

public abstract class ScenarioBase : IScenario
{
    protected ScenarioBase(ISqlExecutor sqlExecutor, ScenarioResourceReader resources, ConsoleInput input, ConsoleOutput output)
    {
        SqlExecutor = sqlExecutor; Resources = resources; Input = input; Output = output;
    }

    protected ISqlExecutor SqlExecutor { get; }
    protected ScenarioResourceReader Resources { get; }
    protected ConsoleInput Input { get; }
    protected ConsoleOutput Output { get; }
    public abstract int Number { get; }
    public abstract string Name { get; }
    protected abstract string SqlFileName { get; }
    protected abstract string NotesFileName { get; }
    protected abstract Task RunAsync();

    public async Task HandleAsync()
    {
        Output.WriteTitle($"Сценарий {Number}. {Name}");
        Output.Write("1 — выполнить сценарий");
        Output.Write("2 — показать SQL-заготовку");
        Output.Write("3 — показать Markdown-заметки");
        Output.Write("0 — назад");

        switch (Input.ReadText("Выберите действие"))
        {
            case "1": await RunAsync(); break;
            case "2": Output.WriteBlock(await Resources.ReadSqlAsync(SqlFileName)); break;
            case "3": Output.WriteBlock(await Resources.ReadNotesAsync(NotesFileName)); break;
        }
    }
}
