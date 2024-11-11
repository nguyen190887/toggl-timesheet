using System.Diagnostics.CodeAnalysis;
using System.IO;
using TogglTimesheet.Timesheet;
using Microsoft.Extensions.Configuration;

namespace TogglTimesheet;

[ExcludeFromCodeCoverage]
public static class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .Build();

        var parameters = ParseArguments(args);
        var outputFile = parameters.GetValueOrDefault("output", "out.csv");
        var taskRulesFile = GetTaskRulesFile(configuration);

        ITaskGenerator taskGenerator = new TaskGenerator(taskRulesFile);
        IDataProvider dataProvider = new FileDataProvider();
        var timesheetGenerator = new TimesheetGenerator(taskGenerator, dataProvider);

        var baseAddress = configuration.GetValue<string>("TogglApi:BaseAddress") ?? "https://localhost";
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
        ITimeDataLoader timeDataLoader = new TimeDataLoader(httpClient);

        try
        {
            if (parameters.ContainsKey("input"))
            {
                // Process local file
                timesheetGenerator.GenerateAndSave(parameters["input"], outputFile);
            }
            else
            {
                // Fetch from API
                if (!parameters.TryGetValue("startDate", out var startDate) ||
                    !parameters.TryGetValue("endDate", out var endDate))
                {
                    Console.WriteLine("Usage: TogglTimesheet --input=<filename> or --startDate=yyyy-MM-dd --endDate=yyyy-MM-dd [--output=<filename>] [--token=<apiToken>] [--workspace=<workspaceId>]");
                    return;
                }

                var apiToken = parameters.GetValueOrDefault("token") ??
                             Environment.GetEnvironmentVariable("TOGGL_API_TOKEN") ??
                             configuration.GetValue<string>("TogglApi:ApiToken");

                var workspaceId = parameters.GetValueOrDefault("workspace") ??
                                 Environment.GetEnvironmentVariable("TOGGL_WORKSPACE_ID") ??
                                 configuration.GetValue<string>("TogglApi:WorkspaceId");

                if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(workspaceId))
                {
                    Console.WriteLine("API token and workspace ID must be provided either via command line arguments or environment variables");
                    return;
                }

                Console.WriteLine($"StartDate: {startDate}, EndDate: {endDate}, Token: {apiToken}, Workspace: {workspaceId}");

                var timeData = await timeDataLoader.FetchTimeDataAsync(apiToken, workspaceId, startDate, endDate);
                var timesheetData = timesheetGenerator.ProcessAndGenerateTimesheet(timeData);
                await File.WriteAllBytesAsync(outputFile, timesheetData.ToArray());
            }

            Console.WriteLine($"Timesheet generated successfully to {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message} {ex.StackTrace}");
            return;
        }

        Console.WriteLine("Press any key to exit!");

        try
        {
            Console.ReadKey();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Console input is not available. Exiting...");
        }
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var parameters = new Dictionary<string, string>();
        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                var equalIndex = arg.IndexOf('=');
                if (equalIndex > 0)
                {
                    var key = arg.Substring(2, equalIndex - 2);
                    var value = arg.Substring(equalIndex + 1).Trim('"');
                    parameters[key] = value;
                }
            }
        }
        return parameters;
    }

    private static string GetTaskRulesFile(IConfiguration configuration)
    {
        var configuredPath = configuration.GetValue<string>("TaskRulesFile");
        if (string.IsNullOrEmpty(configuredPath))
        {
            throw new InvalidOperationException("TaskRulesFile configuration is required");
        }

        if (!File.Exists(configuredPath))
        {
            throw new FileNotFoundException($"Task rules file not found at: {configuredPath}");
        }

        return configuredPath;
    }
}
