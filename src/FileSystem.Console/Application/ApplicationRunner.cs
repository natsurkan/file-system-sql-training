using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Scenarios;

namespace FileSystem.ConsoleApp.Application;

public sealed class ApplicationRunner
{
    private readonly IReadOnlyList<IScenario> _scenarios;
    private readonly ConsoleInput _input;
    private readonly ConsoleOutput _output;

    public ApplicationRunner(IReadOnlyList<IScenario> scenarios, ConsoleInput input, ConsoleOutput output)
    {
        _scenarios = scenarios;
        _input = input;
        _output = output;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            _output.ShowMenu(_scenarios);
            var command = _input.ReadText("Выберите пункт");
            if (command == "0") return;
            if (!int.TryParse(command, out var number) || number < 1 || number > _scenarios.Count)
            {
                _output.WriteError("Неизвестный пункт меню.");
                continue;
            }

            await _scenarios[number - 1].HandleAsync();
            _input.WaitForEnter();
        }
    }
}
