using System.Globalization;
using CsvHelper;
using Moq;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet.Tests.Timesheet
{
    public class FileDataProviderTest
    {
        private readonly string _inputFile = "input.csv";
        private readonly string _outputFile = "output.csv";

        [Fact]
        public void LoadTimeEntries_ShouldReturnListOfTimeEntries()
        {
            // Arrange
            var expectedEntries = new List<TimeEntry>
            {
            new TimeEntry { Description = "Task1", RawDuration = TimeSpan.FromHours(2.5).ToString() },
            new TimeEntry { Description = "Task2", RawDuration = TimeSpan.FromHours(3.0).ToString() }
            };

            // Create mock content and save it to _inputFile
            using (var writer = new StreamWriter(_inputFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(expectedEntries);
            }

            // Act
            var fileDataProvider = new FileDataProvider();
            var result = fileDataProvider.LoadTimeEntries(_inputFile);

            // Assert
            Assert.Equal(expectedEntries.Count, result.Count);
            for (int i = 0; i < expectedEntries.Count; i++)
            {
                Assert.Equal(expectedEntries[i].Description, result[i].Description);
                Assert.Equal(expectedEntries[i].RawDuration, result[i].RawDuration);
            }

            // Clean up
            if (File.Exists(_inputFile))
            {
                File.Delete(_inputFile);
            }
        }

        [Fact]
        public void SaveTimesheet_ShouldWriteEntriesToFile()
        {
            // Arrange
            var mockFileDataProvider = new Mock<FileDataProvider>();
            var entries = new Dictionary<string, ReportedTimeEntry>
            {
                { "Task1", new ReportedTimeEntry { Task = "Task1", DayTime = new Dictionary<DateTime, double> { { DateTime.Today, 2.5 } } } },
                { "Task2", new ReportedTimeEntry { Task = "Task2", DayTime = new Dictionary<DateTime, double> { { DateTime.Today, 3.0 } } } }
            };
            var timesheetDays = new List<DateTime> { DateTime.Today };

            // Act
            var fileDataProvider = new FileDataProvider();
            fileDataProvider.SaveTimesheet(entries, timesheetDays, _outputFile);

            // Assert
            using (var reader = new StreamReader(_outputFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                Assert.Equal(2, records.Count);

                var record1 = ((IDictionary<string, object>)records[0]).ToDictionary(k => k.Key, k => k.Value.ToString());
                var record2 = ((IDictionary<string, object>)records[1]).ToDictionary(k => k.Key, k => k.Value.ToString());

                Assert.Equal("Task1", record1["Task"]);
                Assert.Equal("2.5", record1[DateTime.Today.ToString("yyyy-MM-dd")]);
                Assert.Equal("Task2", record2["Task"]);
                Assert.Equal("3", record2[DateTime.Today.ToString("yyyy-MM-dd")]);
            }
        }
    }
}