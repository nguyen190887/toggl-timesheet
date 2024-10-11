using Microsoft.AspNetCore.Mvc;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetGenerator _timesheetGenerator;

        public TimesheetController(ITimesheetGenerator timesheetGenerator)
        {
            _timesheetGenerator = timesheetGenerator;
        }

        [HttpPost("generate")]
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
    }
}
