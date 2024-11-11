# Toggl Timesheet Generator
Tool to generate tracked time data from [Toggl](https://toggl.com/) Detailed report (CSV format) to tabular format (used by many software companies).

# How to use console?
You can use the console application in two ways:

1. Process a local CSV file:
   ```bash
   # Run as a developer
   dotnet run --input=<toggl_csv_detailed_report_file> [--output=<output_file>]

   # Run as a user
   TogglTimesheet.exe --input=<toggl_csv_detailed_report_file> [--output=<output_file>]
   ```

2. Fetch data directly from Toggl API:
   ```bash
   # Run as a developer
   dotnet run --startDate=yyyy-MM-dd --endDate=yyyy-MM-dd [--output=<output_file>] [--token=<apiToken>] [--workspace=<workspaceId>]

   # Run as a user
   TogglTimesheet.exe --startDate=yyyy-MM-dd --endDate=yyyy-MM-dd [--output=<output_file>] [--token=<apiToken>] [--workspace=<workspaceId>]
   ```
   Note: For API access, you need to provide:
   - API token: via --token parameter or TOGGL_API_TOKEN environment variable
   - Workspace ID: via --workspace parameter or TOGGL_WORKSPACE_ID environment variable

If no output file is specified, the result will be written to `out.csv` by default.

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
