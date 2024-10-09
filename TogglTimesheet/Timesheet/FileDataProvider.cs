using System.Globalization;
using CsvHelper;

namespace TogglTimesheet.Timesheet
{
    public interface IDataProvider
    {
        List<TimeEntry> LoadTimeEntriesFromStream(Stream inputStream);
        List<TimeEntry> LoadTimeEntries(string inputFile);
        void SaveTimesheet(Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays, string outputFile);
        void SaveTimesheetToStream(StreamWriter writer, Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays);
    }

    public class FileDataProvider : IDataProvider
    {
        public List<TimeEntry> LoadTimeEntriesFromStream(Stream inputStream)
        {
            using var reader = new StreamReader(inputStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<TimeEntry>().ToList();
        }
        public List<TimeEntry> LoadTimeEntries(string inputFile)
        {
            using var reader = new StreamReader(inputFile);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<TimeEntry>().ToList();
        }

        public void SaveTimesheet(Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays, string outputFile)
        {
            if (outputFile == null)
            {
                throw new InvalidOperationException("Output file path cannot be null.");
            }

            using var writer = new StreamWriter(outputFile);
            SaveTimesheetToStream(writer, entries, timesheetDays);
        }

        public void SaveTimesheetToStream(StreamWriter writer, Dictionary<string, ReportedTimeEntry> entries, IEnumerable<DateTime> timesheetDays)
        {
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