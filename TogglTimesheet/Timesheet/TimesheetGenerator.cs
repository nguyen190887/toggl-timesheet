// Generated by Copilot
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TogglTimesheet.Timesheet
{
    public interface ITimesheetGenerator
    {
        void GenerateAndSave(string inputFile, string outputFile);
        MemoryStream GenerateData(Stream inputStream);
        MemoryStream ProcessAndGenerateTimesheet(IEnumerable<TimeEntryBase> entries);
        (Dictionary<string, ReportedTimeEntry> TimesheetData, HashSet<DateTime> TimesheetDays, List<(string Description, string Project)> UnknownTasks)
            ProcessEntries(IEnumerable<TimeEntryBase> entries);
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

            var (timesheetData, timesheetDays, unknownTasks) = ProcessEntries(entries);

            _dataProvider.SaveTimesheet(timesheetData, timesheetDays.ToList(), outputFile, unknownTasks);
        }

        public MemoryStream GenerateData(Stream inputStream)
        {
            var entries = _dataProvider.LoadTimeEntriesFromStream(inputStream);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(entries)); // for debugging

            var memoryStream = ProcessAndGenerateTimesheet(entries);
            return memoryStream;
        }

        public MemoryStream ProcessAndGenerateTimesheet(IEnumerable<TimeEntryBase> entries)
        {
            var (timesheetData, timesheetDays, unknownTasks) = ProcessEntries(entries);

            var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
            {
                _dataProvider.SaveTimesheetToStream(streamWriter, timesheetData, timesheetDays.ToList(), unknownTasks);
            }
            memoryStream.Position = 0; // Reset the position to the beginning of the stream
            return memoryStream;
        }

        public (Dictionary<string, ReportedTimeEntry> TimesheetData, HashSet<DateTime> TimesheetDays, List<(string Description, string Project)> UnknownTasks)
            ProcessEntries(IEnumerable<TimeEntryBase> entries)
        {
            var sortedEntries = entries.OrderBy(x => x.StartDate).ThenBy(x => x.Project).ThenBy(x => x.Description);
            var timesheetData = new Dictionary<string, ReportedTimeEntry>();
            var timesheetDays = new HashSet<DateTime>();
            var unknownTasks = new List<(string Description, string Project)>();

            foreach (var entry in sortedEntries)
            {
                timesheetDays.Add(entry.StartDate);
                var task = _taskGenerator.GenerateTask(entry.Description, entry.Project);

                if (task == TaskConstants.Unknown)
                {
                    Console.WriteLine("[Unknown task] {0} (project: {1})", entry.Description, entry.Project);
                    unknownTasks.Add((entry.Description, entry.Project));
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

            return (timesheetData, timesheetDays, unknownTasks);
        }
    }
}
