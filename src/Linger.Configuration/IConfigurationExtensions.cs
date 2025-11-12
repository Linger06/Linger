using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

/// <summary>
/// Provides extension methods for IConfiguration
/// </summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Gets a strongly-typed configuration section
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="sectionName">The section name</param>
    /// <returns>The strongly-typed configuration object, or null if not found</returns>
    public static T? GetGeneric<T>(this IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ArgumentNullException.ThrowIfNull(sectionName);

        IConfigurationSection section = configuration.GetSection(sectionName);

        T? value = section.Get<T>();
        return value;
    }
}