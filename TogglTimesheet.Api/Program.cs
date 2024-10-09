using TogglTimesheet.Timesheet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register dependencies
builder.Services.AddScoped<ITaskGenerator, TaskGenerator>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    // var taskRulesFile = Program.GetTaskRulesFile(configuration);
    // return new TaskGenerator(configuration.GetValue<string>("Toggl:TaskRulesFile") ?? "Config/taskRule.json");
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    var taskRulesFile = Path.Combine(env.ContentRootPath, configuration.GetValue<string>("Toggl:TaskRulesFile") ?? "Config/taskRules.json");
    return new TaskGenerator(taskRulesFile);
});
builder.Services.AddScoped<IDataProvider, FileDataProvider>();
builder.Services.AddScoped<ITimesheetGenerator, TimesheetGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
