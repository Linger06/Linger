using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

/// <summary>
/// Provides static helper methods for accessing application settings.
/// All methods delegate to the shared <see cref="AppConfig.Instance"/> configuration root.
/// </summary>
public static class AppSettingsHelper
{
    private static IConfiguration Configuration => AppConfig.Instance.Config;

    /// <summary>
    /// Gets a configuration section with the specified key
    /// </summary>
    /// <param name="key">The configuration section key</param>
    /// <returns>The configuration section</returns>
    public static IConfigurationSection GetSection(string key)
    {
        return Configuration.GetSection(key);
    }

    /// <summary>
    /// Gets a configuration section value with the specified key
    /// </summary>
    /// <param name="key">The configuration section key</param>
    /// <returns>The section value, or null if not found</returns>
    public static string? GetSectionValue(string key)
    {
        return Configuration.GetSection(key).Value;
    }

    /// <summary>
    /// Gets a connection string with the specified name
    /// </summary>
    /// <param name="key">The connection string name</param>
    /// <returns>The connection string, or null if not found</returns>
    public static string? GetConnectionString(string key)
    {
        return Configuration.GetConnectionString(key);
    }

    /// <summary>
    /// Converts the entire configuration to a strongly-typed object
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <returns>The converted object, or null if conversion fails</returns>
    public static T? ConvertToObject<T>() where T : class
    {
        return Configuration.Get<T>();
    }
}