using System.Diagnostics.CodeAnalysis;
using System.IO;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet;

[ExcludeFromCodeCoverage]
public static class Program
{
    static async Task Main(string[] args)
    {
        var parameters = ParseArguments(args);
        var outputFile = parameters.GetValueOrDefault("output", "out.csv");
        var taskRulesFile = GetTaskRulesFile();

        ITaskGenerator taskGenerator = new TaskGenerator(taskRulesFile);
        IDataProvider dataProvider = new FileDataProvider();
        var timesheetGenerator = new TimesheetGenerator(taskGenerator, dataProvider);
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.track.toggl.com/")
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
                             Environment.GetEnvironmentVariable("TOGGL_API_TOKEN");
                var workspaceId = parameters.GetValueOrDefault("workspace") ??
                                 Environment.GetEnvironmentVariable("TOGGL_WORKSPACE_ID");

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

    private static string GetTaskRulesFile()
    {
        const string localTaskRulesFile = "taskRules.local.json";
        const string defaultTaskRulesFile = "taskRules.json";
        return File.Exists(localTaskRulesFile) ? localTaskRulesFile : defaultTaskRulesFile;
    }
}
