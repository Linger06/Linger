using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

public class AppConfig
{
    private static readonly Lazy<AppConfig> s_instance = new(() => new AppConfig());

    private AppConfig()
    {
        Config = Build();
    }

    public IConfigurationRoot Config { get; }

    public static AppConfig Instance => s_instance.Value;

    private static IConfigurationRoot Build()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), true, true);
        IConfigurationRoot configuration = builder.Build();
        return configuration;
    }
}