using Linger.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linger.Email.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     添加并配置Mail服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
        services.AddTransient<IEmailService, EmailService>();
    }

    /// <summary>
    ///     Configure MailKit
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureMailKit(this IServiceCollection services, IConfiguration configuration)
    {
        EmailConfig? emailConfig = configuration.GetGeneric<EmailConfig>("EmailConfig");

        ArgumentNullException.ThrowIfNull(emailConfig);
        services.AddTransient<IEmailService, EmailService>();
        services.AddSingleton(emailConfig);
    }
}