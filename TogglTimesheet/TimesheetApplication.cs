using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet;

public class TimesheetApplication
{
    private readonly IConfiguration _configuration;

    public TimesheetApplication(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task RunAsync(string[] args)
    {
        var parameters = ParseArguments(args);
        var outputFile = parameters.GetValueOrDefault("output", "out.csv");
        var taskRulesFile = GetTaskRulesFile();

        ITaskGenerator taskGenerator = new TaskGenerator(taskRulesFile);
        IDataProvider dataProvider = new FileDataProvider();
        var timesheetGenerator = new TimesheetGenerator(taskGenerator, dataProvider);

        try
        {
            if (parameters.ContainsKey("input"))
            {
                timesheetGenerator.GenerateAndSave(parameters["input"], outputFile);
            }
            else
            {
                await FetchDataFromApiAsync(parameters, timesheetGenerator, outputFile);
            }

            Console.WriteLine($"Timesheet generated successfully to {outputFile}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate timesheet: {ex.Message}", ex);
        }
    }

    private async Task FetchDataFromApiAsync(
        Dictionary<string, string> parameters,
        TimesheetGenerator timesheetGenerator,
        string outputFile)
    {
        if (!parameters.TryGetValue("startDate", out var startDate) ||
            !parameters.TryGetValue("endDate", out var endDate))
        {
            throw new ArgumentException("Usage: TogglTimesheet --input=<filename> or --startDate=yyyy-MM-dd --endDate=yyyy-MM-dd [--output=<filename>] [--token=<apiToken>] [--workspace=<workspaceId>]");
        }

        var apiToken = GetConfigValue(parameters, "token", "TOGGL_API_TOKEN", "TogglApi:ApiToken");
        var workspaceId = GetConfigValue(parameters, "workspace", "TOGGL_WORKSPACE_ID", "TogglApi:WorkspaceId");

        if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(workspaceId))
        {
            throw new ArgumentException("API token and workspace ID must be provided either via command line arguments or environment variables");
        }

        Console.WriteLine($"StartDate: {startDate}, EndDate: {endDate}, Token: {apiToken}, Workspace: {workspaceId}");

        var baseAddress = _configuration.GetValue<string>("TogglApi:BaseAddress") ?? "https://localhost";
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
        ITimeDataLoader timeDataLoader = new TimeDataLoader(httpClient);

        var timeData = await timeDataLoader.FetchTimeDataAsync(apiToken, workspaceId, startDate, endDate);
        var timesheetData = timesheetGenerator.ProcessAndGenerateTimesheet(timeData);
        await File.WriteAllBytesAsync(outputFile, timesheetData.ToArray());
    }

    private string GetConfigValue(
        Dictionary<string, string> parameters,
        string paramName,
        string envVarName,
        string configKey)
    {
        return parameters.GetValueOrDefault(paramName) ??
               Environment.GetEnvironmentVariable(envVarName) ??
               _configuration.GetValue<string>(configKey) ?? string.Empty;
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

    private string GetTaskRulesFile()
    {
        var configuredPath = _configuration.GetValue<string>("TogglApi:TaskRuleFile");
        if (string.IsNullOrEmpty(configuredPath))
        {
            throw new InvalidOperationException("TaskRuleFile configuration is required");
        }

        if (!File.Exists(configuredPath))
        {
            throw new FileNotFoundException($"Task rules file not found at: {configuredPath}");
        }

        return configuredPath;
    }
}
