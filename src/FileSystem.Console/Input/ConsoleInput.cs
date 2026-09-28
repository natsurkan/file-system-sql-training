namespace FileSystem.ConsoleApp.Input;

public sealed class ConsoleInput
{
    public string ReadText(string prompt) { Console.Write($"{prompt}: "); return Console.ReadLine()?.Trim() ?? string.Empty; }
    public void WaitForEnter() { Console.Write("Нажмите Enter для продолжения..."); Console.ReadLine(); }
}
