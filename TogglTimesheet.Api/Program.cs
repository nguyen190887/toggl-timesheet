using TogglTimesheet.Api.Extensions;
using TogglTimesheet.Timesheet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load configuration based on environment
var environment = builder.Environment;

builder.Configuration
    .SetBasePath(environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddScoped<ITaskGenerator, TaskGenerator>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    var taskRulesFile = Path.Combine(env.ContentRootPath, configuration.GetValue<string>("Toggl:TaskRuleFile") ?? "Config/taskRules.json");
    return new TaskGenerator(taskRulesFile);
});
builder.Services.AddScoped<IDataProvider, FileDataProvider>();
builder.Services.AddScoped<ITimesheetGenerator, TimesheetGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDev())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
