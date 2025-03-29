using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TogglTimesheet.Api.Config;
using TogglTimesheet.Api.Extensions;
using TogglTimesheet.Timesheet;
using TogglTimesheet.Api;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();

// Make the Program class public so it can be used by Lambda
public partial class Program { }
