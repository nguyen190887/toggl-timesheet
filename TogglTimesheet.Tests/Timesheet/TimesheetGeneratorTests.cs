using Moq;

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

            var csvContent = "Start date,Project,Description,Duration\n" +
                "2023-10-01,Project1,Task1,02:30:00\n" +
                "2023-10-01,Project1,Task1,01:00:00\n" +
                "2023-10-02,Project1,Task2,03:00:00\n";

            File.WriteAllText(inputFile, csvContent);

            var mockTaskGenerator = new Mock<ITaskGenerator>();
            mockTaskGenerator.Setup(t => t.GenerateTask(It.IsAny<string>(), It.IsAny<string>())).Returns("TestTask");
            var taskGenerator = mockTaskGenerator.Object;
            var generator = new TimesheetGenerator(inputFile, outputFile, taskGenerator);

            // Act
            generator.GenerateAndSave(taskRuleFile);

            // Assert
            var expectedOutput = $"Task,2023-10-01,2023-10-02{Environment.NewLine}" +
                 $"TestTask,3.5,3{Environment.NewLine}";

            var actualOutput = File.ReadAllText(outputFile);
            Assert.Equal(expectedOutput, actualOutput);

            // Cleanup
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }
}