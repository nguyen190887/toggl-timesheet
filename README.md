# toggl-timesheet
Tool to generate timesheet data from Toggl detailed report (CSV format)
# How to use?
- Export [Toggl](https://track.toggl.com/timer) Detailed report (CSV file is downloaded)
- Run the code (if you're developer) or the binary file using following syntaxes:
  ```bash
  # Run as a developer
  dotnet run <toggl_csv_detailed_report_file>

  # Run as a user
  dotnet toggl-timesheet.dll <toggl_csv_detailed_report_file>
  ```
- Output timesheet data is written to `out.csv` file (this will be parameterized in the next version)
