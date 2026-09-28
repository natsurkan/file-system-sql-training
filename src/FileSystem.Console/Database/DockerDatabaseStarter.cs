using System.Diagnostics;

namespace FileSystem.ConsoleApp.Database;

public sealed class DockerDatabaseStarter
{
    public async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
    {
        var repositoryRoot = FindRepositoryRoot();
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "compose up -d",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Не удалось запустить Docker Compose.");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"Docker Compose завершился с ошибкой: {error}");
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current, "docker-compose.yml"))) return current;
            current = Directory.GetParent(current)?.FullName;
        }

        throw new FileNotFoundException("Не найден docker-compose.yml в проекте.");
    }
}