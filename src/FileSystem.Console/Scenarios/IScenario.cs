namespace FileSystem.ConsoleApp.Scenarios;

public interface IScenario
{
    int Number { get; }
    string Name { get; }
    Task HandleAsync();
}
