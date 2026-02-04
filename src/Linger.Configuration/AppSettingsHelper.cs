using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

/// <summary>
/// Provides static helper methods for accessing application settings
/// </summary>
public static class AppSettingsHelper
{
    private static readonly IConfiguration s_configuration = InitializeConfiguration();

    static IConfiguration InitializeConfiguration()
    {
        //在当前目录或者根目录中寻找 appsettings.json文件
        const string FileName = "appsettings.json";

        //如果你把 配置文件 根据环境变量来分开了，可以这样写
        //fileName = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

        var directory = AppContext.BaseDirectory;

        var filePath = Path.Combine(directory, FileName);
        var builder = new ConfigurationBuilder();
        if (File.Exists(filePath))
        {
            builder.AddJsonFile(filePath, false, true);
        }

        return builder.Build();
    }

    /// <summary>
    /// Gets a configuration section with the specified key
    /// </summary>
    /// <param name="key">The configuration section key</param>
    /// <returns>The configuration section</returns>
    public static IConfigurationSection GetSection(string key)
    {
        return s_configuration.GetSection(key);
    }

    /// <summary>
    /// Gets a configuration section value with the specified key
    /// </summary>
    /// <param name="key">The configuration section key</param>
    /// <returns>The section value, or null if not found</returns>
    public static string? GetSectionValue(string key)
    {
        return s_configuration.GetSection(key).Value;
    }

    /// <summary>
    /// Gets a connection string with the specified name
    /// </summary>
    /// <param name="key">The connection string name</param>
    /// <returns>The connection string, or null if not found</returns>
    public static string? GetConnectionString(string key)
    {
        return s_configuration.GetConnectionString(key);
    }

    /// <summary>
    /// Converts the entire configuration to a strongly-typed object
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <returns>The converted object, or null if conversion fails</returns>
    public static T? ConvertToObject<T>() where T : class
    {
        return s_configuration.Get<T>();
    }

    /// <summary>
    /// Obsolete typo-compatible method. Please use ConvertToObject instead.
    /// </summary>
    [Obsolete("Use ConvertToObject instead of CovertToObject. This method will be removed in a future release.", false)]
    public static T? CovertToObject<T>() where T : class
    {
        return ConvertToObject<T>();
    }
}
