using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace TogglTimesheet.Timesheet.Tests
{
    public class TimeDataLoaderTest
    {
        [Fact]
        public async Task FetchTimeDataAsync_ShouldReturnTimeDataList()
        {
            // Arrange
            var apiToken = "test_api_token";
            var workspaceId = "test_workspace_id";
            var startDate = "2023-01-01";
            var endDate = "2023-01-31";

            var projectResponse = new List<TimeDataLoader.Project>
            {
                new TimeDataLoader.Project { Id = 1, Name = "Project1" },
                new TimeDataLoader.Project { Id = 2, Name = "Project2" }
            };

            dynamic timeEntriesResponseJson = new[]
            {
                new
                {
                    project_id = 1,
                    description = "Task1",
                    time_entries = new[]
                    {
                        new
                        {
                            start = "2023-01-01T08:00:00Z",
                            stop = "2023-01-01T10:30:00Z",
                            seconds = 9000
                        }
                    }
                },
                new
                {
                    project_id = 2,
                    description = "Task2",
                    time_entries = new[]
                    {
                        new
                        {
                            start = "2023-01-02T09:00:00Z",
                            stop = "2023-01-02T12:00:00Z",
                            seconds = 10800
                        }
                    }
                }
            };

            var timeEntriesResponseJsonString = JsonSerializer.Serialize(timeEntriesResponseJson);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/api/v9/workspaces/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(projectResponse)
                });

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/reports/api/v3/workspace/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(timeEntriesResponseJsonString, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            var timeDataLoader = new TimeDataLoader(httpClient);

            // Act
            var result = await timeDataLoader.FetchTimeDataAsync(apiToken, workspaceId, startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal("Project1", result[0].ProjectName);
            Assert.Equal("Task1", result[0].Description);
            Assert.Equal(DateTime.Parse("2023-01-01T08:00:00Z").ToUniversalTime(), result[0].StartDate.ToUniversalTime());
            Assert.Equal(DateTime.Parse("2023-01-01T10:30:00Z").ToUniversalTime(), result[0].EndDate.ToUniversalTime());
            Assert.Equal(2.5, result[0].Duration);

            Assert.Equal("Project2", result[1].ProjectName);
            Assert.Equal("Task2", result[1].Description);
            Assert.Equal(DateTime.Parse("2023-01-02T09:00:00Z").ToUniversalTime(), result[1].StartDate.ToUniversalTime());
            Assert.Equal(DateTime.Parse("2023-01-02T12:00:00Z").ToUniversalTime(), result[1].EndDate.ToUniversalTime());
            Assert.Equal(3.0, result[1].Duration);
        }
    }
}
