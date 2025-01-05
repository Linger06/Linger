using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

public static class AppSettingsHelper
{
    private static readonly IConfiguration s_configuration;

    static AppSettingsHelper()
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

        s_configuration = builder.Build();
    }

    public static IConfigurationSection GetSection(string key)
    {
        return s_configuration.GetSection(key);
    }

    public static string? GetSectionValue(string key)
    {
        return s_configuration.GetSection(key).Value;
    }

    public static string? GetConnectionString(string key)
    {
        return s_configuration.GetConnectionString(key);
    }

    public static T? CovertToObject<T>() where T : class
    {
        return s_configuration.Get<T>();
    }
}