using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Moq;
using Xunit;

namespace TogglTimesheet.Timesheet.Tests
{
    public class TimesheetGeneratorTests
    {
        [Fact]
        public void GenerateAndSave_ShouldGenerateCorrectTimesheet()
            {
            // Arrange
            var inputFile = "input.csv";
            var outputFile = "output.csv";
            var taskRuleFile = "taskRules.json";
            var mockTaskGenerator = new Mock<TaskGenerator>(taskRuleFile);
            mockTaskGenerator.Setup(t => t.GenerateTask(It.IsAny<string>(), It.IsAny<string>())).Returns("TestTask");

            var timeEntries = new List<TimeEntry>
            {
                new TimeEntry { RawStartDate = "2023-10-01", Project = "Project1", Description = "Task1", RawDuration = "2.5" },
                new TimeEntry { RawStartDate = "2023-10-02", Project = "Project1", Description = "Task2", RawDuration = "3.0" }
            };

            var csvContent = "RawStartDate,Project,Description,RawDuration\n" +
                             "2023-10-01,Project1,Task1,2.5\n" +
                             "2023-10-02,Project1,Task2,3.0\n";

            File.WriteAllText(inputFile, csvContent);

            var generator = new TimesheetGenerator(inputFile, outputFile);

            // Act
            generator.GenerateAndSave(taskRuleFile);

            // Assert
            var expectedOutput = "Task,2023-10-01\n" +
                                 "Unknown,2.5\n";

            var actualOutput = File.ReadAllText(outputFile);
            Assert.Equal(expectedOutput, actualOutput);

            // Cleanup
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }
}