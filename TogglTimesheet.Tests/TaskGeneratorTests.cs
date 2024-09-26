using TogglTimesheet.TimesheetGenerators;

namespace TogglTimesheet.Tests
{
    public class TaskGeneratorTests
    {
        private readonly TaskGenerator _taskGenerator;

        public TaskGeneratorTests()
        {
            var taskRules = new List<TaskRule>()
            {
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"interview\")", TaskName = TaskConstants.DC_Interview },
                new() { Expression = "description.StartsWith(\"DC\") && (description.Contains(\"productivity\") || description.Contains(\"initiatives\"))", TaskName = TaskConstants.DC_Productivity },
                new() { Expression = "description.StartsWith(\"DC\") && (description.Contains(\"propo\") || description.Contains(\"pro.\"))", TaskName = TaskConstants.DC_Proposal },
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"support\")", TaskName = TaskConstants.DC_Support },
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"iqbr\")", TaskName = TaskConstants.DC_IQRB },
                new() { Expression = "description.StartsWith(\"A -\") && description.Contains(\"night meeting\")", TaskName = TaskConstants.PRX_NightMeeting },
                new() { Expression = "description.StartsWith(\"A -\")", TaskName = TaskConstants.PRX_Tasks },
                new() { Expression = "description.StartsWith(\"Atd -\") && description.Contains(\"night meeting\")", TaskName = TaskConstants.ATD_NightMeeting },
                new() { Expression = "description.StartsWith(\"Atd -\")", TaskName = TaskConstants.ATD_Tasks },
                new() { Expression = "project == \"Self-Development\" || description.ContainsIgnoreCase(\"learning\")", TaskName = TaskConstants.Learning },
                new() { Expression = "project == \"Innovation\"", TaskName = TaskConstants.Innovation }
            };
            _taskGenerator = new TaskGenerator(taskRules);
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