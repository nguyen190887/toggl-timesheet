using TogglTimesheet.TimesheetGenerators;

namespace TogglTimesheet.Tests
{
    public class TaskGeneratorTests
    {
        private readonly TaskGenerator _taskGenerator;

        public TaskGeneratorTests()
        {
            _taskGenerator = new TaskGenerator();
        }

        [Theory]
        [InlineData("DC - interview: case 1", "", TaskConstants.DC_Interview)]
        [InlineData("DC - productivity: meeting", "", TaskConstants.DC_Productivity)]
        [InlineData("DC - initiatives: activity", "", TaskConstants.DC_Productivity)]
        [InlineData("DC - propo__", "", TaskConstants.DC_Proposal)]
        [InlineData("DC - pro. - proj X", "", TaskConstants.DC_Proposal)]
        [InlineData("DC - support", "", TaskConstants.DC_Support)]
        [InlineData("DC - iqbr", "", TaskConstants.DC_IQRB)]
        [InlineData("A - night meeting", "", TaskConstants.PRX_NightMeeting)]
        [InlineData("A - some task", "", TaskConstants.PRX_Tasks)]
        [InlineData("Atd - night meeting", "", TaskConstants.ATD_NightMeeting)]
        [InlineData("Atd - some task", "", TaskConstants.ATD_Tasks)]
        [InlineData("Some description", "Self-Development", TaskConstants.Learning)]
        [InlineData("Learning something new", "", TaskConstants.Learning)]
        [InlineData("Some description", "Innovation", TaskConstants.Innovation)]
        [InlineData("Unknown task", "Unknown project", TaskConstants.Unknown)]
        public void GenerateTask_ShouldReturnExpectedTaskName(string description, string project, string expectedTaskName)
        {
            // Act
            var result = _taskGenerator.GenerateTask(description, project);

            // Assert
            Assert.Equal(expectedTaskName, result);
        }
    }
}