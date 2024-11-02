using System.Diagnostics.CodeAnalysis;
using System.IO;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet;

[ExcludeFromCodeCoverage]
public static class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TogglTimesheet <filename> [<output>]");
            return;
        }
        var inputFile = args[0];
        var outputFile = args.Length > 1 ? args[1] : "out.csv";
        var taskRulesFile = GetTaskRulesFile();
        ITaskGenerator taskGenerator = new TaskGenerator(taskRulesFile); // Assuming TaskGenerator implements ITaskGenerator
        IDataProvider dataProvider = new FileDataProvider(); // No parameters needed
        var timesheetGenerator = new TimesheetGenerator(taskGenerator, dataProvider);

        try
        {
            timesheetGenerator.GenerateAndSave(inputFile, outputFile); // Ensure GenerateAndSave method uses inputFile and outputFile
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
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

    private static string GetTaskRulesFile()
    {
        const string localTaskRulesFile = "taskRules.local.json";
        const string defaultTaskRulesFile = "taskRules.json";
        return File.Exists(localTaskRulesFile) ? localTaskRulesFile : defaultTaskRulesFile;
    }
}
