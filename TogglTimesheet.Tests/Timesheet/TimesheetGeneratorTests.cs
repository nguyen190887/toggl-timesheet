using System.Diagnostics.CodeAnalysis;
using Moq;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet.Tests.Timesheet
{
    [ExcludeFromCodeCoverage]
    public class TimesheetGeneratorTests
    {
        [Fact]
        public void GenerateAndSave_ShouldGenerateAndSaveTimesheetCorrectly()
        {
            // Arrange
            var mockTaskGenerator = new Mock<ITaskGenerator>();
            var mockDataProvider = new Mock<IDataProvider>();

            var timeEntries = new List<TimeEntry>
            {
                new TimeEntry { RawStartDate = "2023-10-01", RawDuration = "02:30:00", Project = "ProjectA", Description = "Task1" },
                new TimeEntry { RawStartDate = "2023-10-01", RawDuration = "01:00:00", Project = "ProjectA", Description = "Task2" },
                new TimeEntry { RawStartDate = "2023-10-02", RawDuration = "03:00:00", Project = "ProjectB", Description = "Task1" }
            };

            mockDataProvider.Setup(dp => dp.LoadTimeEntries(It.IsAny<string>())).Returns(timeEntries);
            mockTaskGenerator.Setup(tg => tg.GenerateTask(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((desc, proj) => desc);

            var timesheetGenerator = new TimesheetGenerator(mockTaskGenerator.Object, mockDataProvider.Object);

            // Act
            var inputFile = "path/to/timeentries.csv";
            var outputFile = "path/to/output.csv";
            timesheetGenerator.GenerateAndSave(inputFile, outputFile);

            // Assert
            mockDataProvider.Verify(dp => dp.LoadTimeEntries(It.IsAny<string>()), Times.Once);

            mockDataProvider.Verify(dp => dp.SaveTimesheet(
                It.Is<Dictionary<string, ReportedTimeEntry>>(dict =>
                    dict.Count == 2 &&
                    dict["Task1"].DayTime[DateTime.Parse("2023-10-01")] == 2.5 &&
                    dict["Task1"].DayTime[DateTime.Parse("2023-10-02")] == 3 &&
                    dict["Task2"].DayTime[DateTime.Parse("2023-10-01")] == 1
                ),
                It.Is<List<DateTime>>(dates =>
                    dates.Count == 2 &&
                    dates.Contains(DateTime.Parse("2023-10-01")) &&
                    dates.Contains(DateTime.Parse("2023-10-02"))
                ),
                It.IsAny<string>()
            ), Times.Once);
        }
    }
}