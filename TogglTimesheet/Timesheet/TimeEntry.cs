// Generated by Copilot
using System.Globalization;
using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

namespace TogglTimesheet.Timesheet
{
    public abstract class TimeEntryBase
    {
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual double Duration { get; set; }
        public abstract string Project { get; set; }
        public abstract string Description { get; set; }
    }

    public class TimeEntry : TimeEntryBase
    {
        private const string DateFormat = "yyyy-MM-dd";

        [Name("Start date")]
        [JsonIgnore]
        public string RawStartDate { get; set; } = string.Empty;

        [Ignore]
        public override DateTime StartDate
        {
            get => DateTime.TryParseExact(
                RawStartDate,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedStartDate)
                ? parsedStartDate
                : DateTime.MinValue;
            set { /* Do nothing */ }
        }

        [Name("End date")]
        [JsonIgnore]
        public string RawEndDate { get; set; } = string.Empty;

        [Ignore]
        public override DateTime EndDate
        {
            get => DateTime.TryParseExact(
                RawEndDate,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedEndDate)
                ? parsedEndDate
                : DateTime.MinValue;
            set { /* Do nothing */ }
        }

        [Name("Duration")]
        [JsonIgnore]
        public string RawDuration { get; set; } = string.Empty;

        [Ignore]
        public override double Duration
        {
            get => TimeSpan.TryParse(RawDuration, out var parsedDuration) ? parsedDuration.TotalHours : 0;
            set { /* Do nothing */ }
        }

        [JsonPropertyName("project")]
        public override string Project { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public override string Description { get; set; } = string.Empty;
    }

    public class ReportedTimeEntry
    {
        public string Task { get; set; } = string.Empty;
        public Dictionary<DateTime, double> DayTime { get; set; } = new();
    }
}
