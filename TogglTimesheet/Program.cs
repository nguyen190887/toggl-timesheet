
using TogglTimesheet.Timesheet;

namespace TogglTimesheet;

public class Program
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
        var timesheetGenerator = new TimesheetGenerator(inputFile, outputFile);

        try
        {
            timesheetGenerator.GenerateAndSave(taskRulesFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit!");
        Console.ReadKey();
    }

    private static string GetTaskRulesFile()
    {
        const string localTaskRulesFile = "taskRules.local.json";
        const string defaultTaskRulesFile = "taskRules.json";

        return File.Exists(localTaskRulesFile) ? localTaskRulesFile : defaultTaskRulesFile;
    }
}
