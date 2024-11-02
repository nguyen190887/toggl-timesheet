// Generated by Copilot
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TogglTimesheet.Timesheet
{
    public class JsonTimeEntry : TimeEntryBase
    {
        [JsonPropertyName("description")]
        public override string Description { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public override double Duration { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public override DateTime StartDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public override DateTime EndDate { get; set; }

        [JsonPropertyName("project_id")]
        public long? ProjectId { get; set; }

        [JsonIgnore]
        public override string Project { get; set; } = string.Empty;

        [JsonPropertyName("time_entries")]
        public List<TimeEntryDetail>? JsonTimeEntries
        {
            get => null; // to allow JSON serialization process only
            set
            {
                if (value != null && value.Count > 0)
                {
                    var entry = value[0];
                    Duration = entry.Seconds / 3600.0;
                    StartDate = entry.Start;
                    EndDate = entry.Stop;
                }
            }
        }
    }

    public class TimeEntryDetail
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("seconds")]
        public int Seconds { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("stop")]
        public DateTime Stop { get; set; }
    }
}
