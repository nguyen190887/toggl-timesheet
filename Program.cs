using System.Globalization;
using CsvHelper;

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

        var entries = LoadTimeEntries(inputFile);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(entries)); // for debugging

        var sortedEntries = entries.OrderBy(x => x.StartDate).ThenBy(x => x.Project).ThenBy(x => x.Description);
        var timesheetData = new Dictionary<string, ReportedTimeEntry>();
        var timesheetDays = new HashSet<DateTime>();

        foreach (var entry in sortedEntries)
        {
            timesheetDays.Add(entry.StartDate);
            var task = GenerateTask(entry.Description, entry.Project);
            if (timesheetData.TryGetValue(task, out var timeData))
            {
                if (timeData.DayTime.ContainsKey(entry.StartDate))
                {
                    timeData.DayTime[entry.StartDate] += entry.Duration;
                }
                else
                {
                    timeData.DayTime[entry.StartDate] = entry.Duration;
                }
            }
            else
            {
                timesheetData[task] = new ReportedTimeEntry
                {
                    Task = task,
                    DayTime = new Dictionary<DateTime, double> { { entry.StartDate, entry.Duration } }
                };
            }
        }

        SaveTimesheet("out.csv", timesheetData, timesheetDays.ToList());
    }

    static List<TimeEntry> LoadTimeEntries(string inputFile)
    {
        List<TimeEntry> loadedEntries;
        using (var reader = new StreamReader(inputFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            loadedEntries = csv.GetRecords<TimeEntry>().ToList();
        }
        return loadedEntries;
    }

    static void SaveTimesheet(string outputFile, Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays)
    {
        using var writer = new StreamWriter(outputFile);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        var headers = timesheetDays.Select(x => x.ToString("yyyy-MM-dd")).ToList();
        headers.Insert(0, "Task");
        csv.WriteField(headers);
        csv.NextRecord();

        foreach (var entry in entries)
        {
            var timeEntry = entry.Value;
            csv.WriteField(timeEntry.Task);
            foreach (var day in timesheetDays)
            {
                if (!timeEntry.DayTime.TryGetValue(day, out var duration))
                {
                    duration = 0;
                }
                csv.WriteField(Math.Round(duration, 2).ToString());
            }
            csv.NextRecord();
        }
    }

    static string GenerateTask(string description, string project)
    {
        if (description.StartsWith("DC") && description.Contains("interview", StringComparison.OrdinalIgnoreCase))
        {
            return "DC - Itv";
        }

        if (description.StartsWith("DC") && description.Contains("productivity", StringComparison.OrdinalIgnoreCase))
        {
            return "DC - 15% Prd";
        }

        if (description.StartsWith("DC") &&
            (description.Contains("propo", StringComparison.OrdinalIgnoreCase) || description.Contains("pro.", StringComparison.OrdinalIgnoreCase)))
        {
            return "DC - Pro.";
        }

        if (description.StartsWith("DC") &&
            description.Contains("omni", StringComparison.OrdinalIgnoreCase))
        {
            return "DC - Support";
        }

        if (description.StartsWith("A -"))
        {
            return "Prx - tasks";
        }

        if (description.StartsWith("Atd -"))
        {
            return "Atd - tasks";
        }

        if (project.Equals("Self-Development", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("learning", StringComparison.OrdinalIgnoreCase))
        {
            return "Learning";
        }

        if (project.Equals("Innovation", StringComparison.OrdinalIgnoreCase))
        {
            return "Innovation";
        }

        Console.WriteLine("[Unknown task] {0} (project: {1})", description, project);
        return "Unknown";
    }
}
