# Toggl Timesheet Generator
Tool to generate tracked time data from [Toggl](https://toggl.com/) Detailed report (CSV format) to tabular format (used by many software companies).

# How to use console?
- Export [Toggl](https://track.toggl.com/timer) Detailed report (CSV file is downloaded)
- Build the exe file by compiling the TogglTimesheet project
- Run the code (if you're developer) or the binary file using following syntaxes:
  ```bash
  # Run as a developer
  dotnet run <toggl_csv_detailed_report_file> [<output_file>]

  # Run as a user
  dotnet toggl-timesheet.dll <toggl_csv_detailed_report_file> [<output_file>]
  ```
- If no output file is specified, the result will be written to `out.csv` by default

# How to use API?

Run the TogglTimesheet.Api project, then you can use the API to generate time reports in these ways:

1. Upload CSV file:
   ```
   POST /api/timesheet/generate
   Content-Type: multipart/form-data
   Body: csvFile=@your_toggl_report.csv
   ```

2. Download time report directly from Toggl:
   ```
   GET /api/timesheet/download-time-report?startDate={yyyy-MM-dd}&endDate={yyyy-MM-dd}
   
   Optional query parameters:
   - apiToken: Your Toggl API token
   - workspaceId: Your Toggl workspace ID
   ```
   If apiToken and workspaceId are not provided, the values from configuration will be used.
