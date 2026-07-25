using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linger.Email.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures the ASP.NET Core email service integration.
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

}
