
using TogglTimesheet.TimesheetGenerators;

namespace TogglTimesheet;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TogglTimesheet <filename>");
            return;
        }
        var inputFile = args[0];

        var timesheetGenerator = new TimesheetGenerator();
        timesheetGenerator.GenerateAndSave(inputFile);

        Console.WriteLine("Press any key to exit!");
        Console.ReadKey();
    }
}
