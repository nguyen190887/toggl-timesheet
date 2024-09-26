using System.Globalization;
using CsvHelper;

namespace TogglTimesheet.TimesheetGenerators
{
    public class TimesheetGenerator
    {
        public List<TimeEntry> LoadTimeEntries(string inputFile)
        {
            List<TimeEntry> loadedEntries;
            using (var reader = new StreamReader(inputFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                loadedEntries = csv.GetRecords<TimeEntry>().ToList();
            }
            return loadedEntries;
        }

        public void SaveTimesheet(string outputFile, Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays)
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

        public void GenerateAndSave(string inputFile)
        {
            var entries = LoadTimeEntries(inputFile);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(entries)); // for debugging

            var sortedEntries = entries.OrderBy(x => x.StartDate).ThenBy(x => x.Project).ThenBy(x => x.Description);
            var timesheetData = new Dictionary<string, ReportedTimeEntry>();
            var timesheetDays = new HashSet<DateTime>();
            var taskGenerator = new TaskGenerator();
            foreach (var entry in sortedEntries)
            {
                timesheetDays.Add(entry.StartDate);
                var task = taskGenerator.GenerateTask(entry.Description, entry.Project);
                
                if (task == TaskConstants.Unknown)
                {
                    Console.WriteLine("[Unknown task] {0} (project: {1})", entry.Description, entry.Project);
                }

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
    }
}