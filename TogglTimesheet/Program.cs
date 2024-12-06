using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace TogglTimesheet;

[ExcludeFromCodeCoverage]
public static class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .Build();

        try
        {
            var application = new TimesheetApplication(configuration);
            await application.RunAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            return;
        }

        Console.WriteLine("Press any key to exit!");

        try
        {
            Console.ReadKey();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Console input is not available. Exiting...");
        }
    }
}
