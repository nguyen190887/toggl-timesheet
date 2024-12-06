using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace TogglTimesheet.Tests
{
    public class TimesheetApplicationTests : IDisposable
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _taskRuleSection;
        private readonly Mock<IConfigurationSection> _baseAddressSection;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly string _testRulesFile;
        private readonly string _testInputFile;
        private readonly string _testOutputFile;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public TimesheetApplicationTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _taskRuleSection = new Mock<IConfigurationSection>();
            _baseAddressSection = new Mock<IConfigurationSection>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Create test files
            _testRulesFile = Path.GetFullPath("test_rules.json"); // Use full path from the start
            _testInputFile = "test_input.csv";
            _testOutputFile = "test_output.csv";

            // Create sample rules file with valid JSON array structure
            var taskRules = @"[
                {
                    ""Expression"": ""description.StartsWith(\""desc\"") && description.Contains(\""itv\"")"",
                    ""TaskName"": ""Interview""
                },
                {
                    ""Expression"": ""description.ContainsIgnoreCase(\""desc\"") && (description.Contains(\""abc\"") || description.Contains(\""xyz\""))"",
                    ""TaskName"": ""Others""
                }
            ]";
            File.WriteAllText(_testRulesFile, taskRules);

            // Create sample input file as CSV with required columns
            File.WriteAllText(_testInputFile,
                "Start date,End date,Duration,Project,Description\n" +
                "2023-01-01,2023-01-01,08:00:00,Test Project,Test Description\n" +
                "2023-01-02,2023-01-02,04:30:00,Test Project,Another Task");

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        }

        public void Dispose()
        {
            // Clean up test files
            if (File.Exists(_testRulesFile)) File.Delete(_testRulesFile);
            if (File.Exists(_testInputFile)) File.Delete(_testInputFile);
            if (File.Exists(_testOutputFile)) File.Delete(_testOutputFile);
        }

        [Fact]
        public async Task RunAsync_WithInputFile_GeneratesTimesheet()
        {
            // Arrange
            SetupTaskRulesConfig(_testRulesFile); // Use the file path that was created in constructor
            var app = new TimesheetApplication(_configurationMock.Object);
            var args = new[] { $"--input={_testInputFile}", $"--output={_testOutputFile}" };

            // Act & Assert
            await app.RunAsync(args);
            Assert.True(File.Exists(_testOutputFile));
        }

        [Fact]
        public async Task RunAsync_WithApiParameters_FetchesAndGeneratesTimesheet()
        {
            // Arrange
            SetupTaskRulesConfig(_testRulesFile);
            SetupApiConfig();

            // Setup mock responses
            var sampleTimeEntries = @"[
                {
                    ""start"": ""2023-01-01T08:00:00+00:00"",
                    ""end"": ""2023-01-01T16:00:00+00:00"",
                    ""duration"": 28800,
                    ""project"": ""Test Project"",
                    ""description"": ""Test Description""
                }
            ]";

            var sampleProjects = @"[
                {
                    ""id"": 1,
                    ""name"": ""Test Project""
                }
            ]";

            SetupMockHttpClient(sampleTimeEntries, sampleProjects);

            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(_mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://api.toggl.com") });

            var app = new TimesheetApplication(_configurationMock.Object, _httpClientFactoryMock.Object);
            var args = new[]
            {
                "--startDate=2023-01-01",
                "--endDate=2023-01-31",
                "--token=test-token",
                "--workspace=123",
                "--output=result.csv"
            };

            // Act
            await app.RunAsync(args);

            // Assert
            Assert.True(File.Exists("result.csv"));
            VerifyHttpClientCalled();
        }

        [Fact]
        public async Task RunAsync_WithoutRequiredParams_ThrowsArgumentException()
        {
            // Arrange
            SetupTaskRulesConfig(_testRulesFile);  // Changed from "rules.json"
            var app = new TimesheetApplication(_configurationMock.Object);
            var args = new[] { "--output=result.csv" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => app.RunAsync(args));
        }

        [Fact]
        public async Task RunAsync_WithMissingTaskRulesFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = Path.GetFullPath("nonexistent_rules.json");
            SetupTaskRulesConfig(nonExistentFile);  // Use full path for consistency
            var app = new TimesheetApplication(_configurationMock.Object);
            var args = new[] { $"--input={_testInputFile}" };

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => app.RunAsync(args));
        }

        [Fact]
        public async Task RunAsync_WithMissingApiCredentials_ThrowsArgumentException()
        {
            // Arrange
            SetupTaskRulesConfig(_testRulesFile);  // Changed from "rules.json"
            var app = new TimesheetApplication(_configurationMock.Object);
            var args = new[]
            {
                "--startDate=2023-01-01",
                "--endDate=2023-01-31"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => app.RunAsync(args));
        }

        private HttpClient CreateMockHttpClient()
        {
            return new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.toggl.com")
            };
        }

        private void SetupMockHttpClient(string timeEntriesResponse, string projectsResponse)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/reports/api/v3/workspace/123/search/time_entries")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(timeEntriesResponse)
                });

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains("/api/v9/workspaces/123/projects")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(projectsResponse)
                });
        }

        private void VerifyHttpClientCalled()
        {
            // Verify time entries API call
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/reports/api/v3/workspace/123/search/time_entries")),
                    ItExpr.IsAny<CancellationToken>()
                );

            // Verify projects API call
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains("/api/v9/workspaces/123/projects")),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        private void SetupTaskRulesConfig(string filePath)
        {
            _taskRuleSection.Setup(x => x.Value).Returns(filePath);
            _configurationMock
                .Setup(x => x.GetSection("TogglApi:TaskRuleFile"))
                .Returns(_taskRuleSection.Object);
        }

        private void SetupApiConfig()
        {
            _baseAddressSection.Setup(x => x.Value).Returns("https://api.toggl.com");
            _configurationMock
                .Setup(x => x.GetSection("TogglApi:BaseAddress"))
                .Returns(_baseAddressSection.Object);
        }
    }
}
