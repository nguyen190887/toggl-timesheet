namespace TogglTimesheet.Api.Extensions;

public static class HostEnvironmentEnvExtensions
{
    /// <summary>
    /// Checks if the current environment is "Local" or "Development".
    /// </summary>
    /// <param name="hostEnvironment">The IHostEnvironment instance.</param>
    /// <returns>True if the environment is "Local"; otherwise, false.</returns>
    public static bool IsDev(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment("Local") || hostEnvironment.IsDevelopment();
    }
}
