using System.Text;
using FileSystem.ConsoleApp.Application;
using FileSystem.ConsoleApp.Database;
using FileSystem.ConsoleApp.Input;
using FileSystem.ConsoleApp.Output;
using FileSystem.ConsoleApp.Resources;
using FileSystem.ConsoleApp.Scenarios;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var databaseStarter = new DockerDatabaseStarter();
await databaseStarter.EnsureStartedAsync();

var connectionFactory = new NpgsqlConnectionFactory();
var sqlExecutor = new NpgsqlSqlExecutor(connectionFactory);
var resources = new ScenarioResourceReader();
var input = new ConsoleInput();
var output = new ConsoleOutput();
var scenarios = ScenarioCatalog.CreateDefault(sqlExecutor, resources, input, output);
var application = new ApplicationRunner(scenarios, input, output);
await application.RunAsync();