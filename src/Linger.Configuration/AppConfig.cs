using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

public class AppConfig
{
    private static AppConfig? s_instance;

    private AppConfig()
    {
        Config = Build();
    }

    public IConfigurationRoot Config { get; }

    public static AppConfig Instance
    {
        get
        {
            s_instance ??= new AppConfig();
            return s_instance;
        }
    }

    private static IConfigurationRoot Build()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), true, true);
        IConfigurationRoot configuration = builder.Build();
        return configuration;
    }
}