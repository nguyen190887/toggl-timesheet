using System.Globalization;
using CsvHelper;

namespace TogglTimesheet.Timesheet
{

    public interface ITimesheetGenerator
    {
        void GenerateAndSave(string inputFile, string outputFile);
        MemoryStream GenerateData(Stream inputStream);
        (Dictionary<string, ReportedTimeEntry> TimesheetData, HashSet<DateTime> TimesheetDays) ProcessEntries(IEnumerable<TimeEntry> entries);
    }

    public class TimesheetGenerator : ITimesheetGenerator

    {
        private readonly ITaskGenerator _taskGenerator;
        private readonly IDataProvider _dataProvider;

        public TimesheetGenerator(ITaskGenerator taskGenerator, IDataProvider dataProvider)
        {
            _taskGenerator = taskGenerator;
            _dataProvider = dataProvider;
        }

        public void GenerateAndSave(string inputFile, string outputFile)
        {
            var entries = _dataProvider.LoadTimeEntries(inputFile);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(entries)); // for debugging

            var (timesheetData, timesheetDays) = ProcessEntries(entries);

            _dataProvider.SaveTimesheet(timesheetData, timesheetDays.ToList(), outputFile);
        }

        public MemoryStream GenerateData(Stream inputStream)
        {
            var entries = _dataProvider.LoadTimeEntriesFromStream(inputStream);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(entries)); // for debugging

            var (timesheetData, timesheetDays) = ProcessEntries(entries);

            var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
            {
                _dataProvider.SaveTimesheetToStream(streamWriter, timesheetData, timesheetDays.ToList());
            }
            memoryStream.Position = 0; // Reset the position to the beginning of the stream
            return memoryStream;
        }

        public (Dictionary<string, ReportedTimeEntry> TimesheetData, HashSet<DateTime> TimesheetDays) ProcessEntries(IEnumerable<TimeEntry> entries)
        {
            var sortedEntries = entries.OrderBy(x => x.StartDate).ThenBy(x => x.Project).ThenBy(x => x.Description);
            var timesheetData = new Dictionary<string, ReportedTimeEntry>();
            var timesheetDays = new HashSet<DateTime>();

            foreach (var entry in sortedEntries)
            {
                timesheetDays.Add(entry.StartDate);
                var task = _taskGenerator.GenerateTask(entry.Description, entry.Project);

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

            return (timesheetData, timesheetDays);
        }
    }
}