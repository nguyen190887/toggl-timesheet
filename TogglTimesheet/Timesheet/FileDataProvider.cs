using System.Globalization;
using CsvHelper;

namespace TogglTimesheet.Timesheet
{
    public interface IDataProvider
    {
        List<TimeEntry> LoadTimeEntries();
        void SaveTimesheet(Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays);
    }

    public class FileDataProvider : IDataProvider
    {
        private readonly string _inputFile;
        private readonly string _outputFile;

        public FileDataProvider(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
        }

        public List<TimeEntry> LoadTimeEntries()
        {
            using var reader = new StreamReader(_inputFile);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<TimeEntry>().ToList();
        }

        public void SaveTimesheet(Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays)
        {
            using var writer = new StreamWriter(_outputFile);
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
    }
}