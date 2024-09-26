using System.Globalization;
using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

namespace TogglTimesheet.TimesheetGenerators
{
    public class TimeEntry
    {
        public string Project { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Name("Start date")]
        [JsonIgnore]
        public string RawStartDate { get; set; } = string.Empty;
        public DateTime StartDate => DateTime.TryParseExact(
                            RawStartDate,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedStartDate)
                        ? parsedStartDate
                        : DateTime.MinValue;
        [Name("Duration")]
        [JsonIgnore]
        public string RawDuration { get; set; } = string.Empty;
        public double Duration => TimeSpan.Parse(RawDuration).TotalHours;
    }

    public class ReportedTimeEntry
    {
        public string Task { get; set; } = string.Empty;
        public Dictionary<DateTime, double> DayTime = [];
        // public double Mon { get; set; }
        // public double Tue { get; set; }
        // public double Wed { get; set; }
        // public double Thu { get; set; }
        // public double Fri { get; set; }
        // public double Weekend { get; set; }
    }
}