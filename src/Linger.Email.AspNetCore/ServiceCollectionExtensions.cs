using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linger.Email.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures email services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration containing email settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
        services.AddTransient<IEmailService, EmailService>();
        return services;
    }

    /// <summary>
    /// Registers and configures email services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration containing email settings</param>
    [Obsolete("Use AddEmailService instead. This method will be removed in a future version.")]
    public static void ConfigureEmail(this IServiceCollection services, IConfiguration configuration)
    {
        AddEmailService(services, configuration);
    }

    /// <summary>
    /// Registers and configures email services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration containing email settings</param>
    [Obsolete("Use AddEmailService instead. This method will be removed in a future version.")]
    public static void ConfigureMailKit(this IServiceCollection services, IConfiguration configuration)
    {
        AddEmailService(services, configuration);
    }
}