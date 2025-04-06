using System.Text.Json.Serialization;
using TogglTimesheet.Api.Config;
using TogglTimesheet.Api.Extensions;
using TogglTimesheet.Timesheet;

namespace TogglTimesheet.Api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register TogglConfig options
        services.Configure<TogglConfig>(Configuration.GetSection("Toggl"));

        services.AddScoped<ITaskGenerator, TaskGenerator>(provider =>
        {
            var env = provider.GetRequiredService<IWebHostEnvironment>();
            var taskRulesFile = Path.Combine(env.ContentRootPath, Configuration.GetValue<string>("Toggl:TaskRuleFile") ?? "Config/taskRules.json");
            return new TaskGenerator(taskRulesFile);
        });

        services.AddScoped<IDataProvider, FileDataProvider>();
        services.AddScoped<ITimesheetGenerator, TimesheetGenerator>();
        services.AddHttpClient<ITimeDataLoader, TimeDataLoader>(client =>
        {
            var baseUrl = Configuration.GetValue<string>("Toggl:BaseUrl");
            client.BaseAddress = new Uri(baseUrl ?? string.Empty);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
