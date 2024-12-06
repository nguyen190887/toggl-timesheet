using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TogglTimesheet.Api.Config;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/octet-stream")]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetGenerator _timesheetGenerator;
        private readonly ITimeDataLoader _timeDataLoader;
        private readonly IOptions<TogglConfig> _togglConfig;

        public TimesheetController(ITimesheetGenerator timesheetGenerator, ITimeDataLoader timeDataLoader, IOptions<TogglConfig> togglConfig)
        {
            _timesheetGenerator = timesheetGenerator;
            _timeDataLoader = timeDataLoader;
            _togglConfig = togglConfig;
        }

        /// <summary>
        /// Generates a timesheet from an uploaded CSV file
        /// </summary>
        /// <param name="csvFile">The CSV file containing time entries</param>
        /// <returns>A generated timesheet file in CSV format</returns>
        /// <response code="200">Returns the generated timesheet CSV file</response>
        /// <response code="400">If the CSV file is missing or empty</response>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GenerateTimesheet([FromForm] IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest("CSV file is required.");
            }

            using var stream = csvFile.OpenReadStream();
            var timesheetData = _timesheetGenerator.GenerateData(stream);
            return File(timesheetData, "application/octet-stream", "timesheet.csv");
        }

        /// <summary>
        /// Loads time entries from Toggl within the specified date range
        /// </summary>
        /// <param name="apiToken">Optional Toggl API token</param>
        /// <param name="workspaceId">Optional Toggl workspace ID</param>
        /// <param name="startDate">Start date for time entries</param>
        /// <param name="endDate">End date for time entries</param>
        /// <returns>List of time entries from Toggl</returns>
        /// <response code="200">Returns the list of time entries</response>
        /// <response code="400">If the dates are invalid or API call fails</response>
        [HttpGet("load-time")]
        [ProducesResponseType(typeof(IEnumerable<TimeEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoadTimeEntries(
            [FromQuery] string? apiToken,
            [FromQuery] string? workspaceId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            const string DateFormat = "yyyy-MM-dd";

            if (string.IsNullOrEmpty(apiToken))
            {
                apiToken = _togglConfig.Value.ApiToken;
            }

            if (string.IsNullOrEmpty(workspaceId))
            {
                workspaceId = _togglConfig.Value.WorkspaceId;
            }

            string startDateString = startDate.ToString(DateFormat);
            string endDateString = endDate.ToString(DateFormat);

            var timeData = await _timeDataLoader.FetchTimeDataAsync(apiToken, workspaceId, startDateString, endDateString);
            return Ok(timeData);
        }

        /// <summary>
        /// Downloads a formatted time report for the specified date range
        /// </summary>
        /// <param name="apiToken">Optional Toggl API token</param>
        /// <param name="workspaceId">Optional Toggl workspace ID</param>
        /// <param name="startDate">Start date for time entries</param>
        /// <param name="endDate">End date for time entries</param>
        /// <returns>A generated time report file in CSV format</returns>
        /// <response code="200">Returns the generated time report CSV file</response>
        /// <response code="400">If the dates are invalid or API call fails</response>
        [HttpGet("download-time-report")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadTimeReport(
            [FromQuery] string? apiToken,
            [FromQuery] string? workspaceId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            const string DateFormat = "yyyy-MM-dd";

            if (string.IsNullOrEmpty(apiToken))
            {
                apiToken = _togglConfig.Value.ApiToken;
            }

            if (string.IsNullOrEmpty(workspaceId))
            {
                workspaceId = _togglConfig.Value.WorkspaceId;
            }

            string startDateString = startDate.ToString(DateFormat);
            string endDateString = endDate.ToString(DateFormat);

            var timeData = await _timeDataLoader.FetchTimeDataAsync(apiToken, workspaceId, startDateString, endDateString);
            var timesheetData = _timesheetGenerator.ProcessAndGenerateTimesheet(timeData);

            return File(timesheetData, "application/octet-stream", "time_report.csv");
        }
    }
}
