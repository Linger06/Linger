using Linger.AspNetCore.Jwt.Contracts;
using Linger.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

public static class ServiceCollectionExtensions
{
    private const string TokenExpiredHeader = "Token-Expired";
    private const string TokenInvalidAudienceHeader = "Token-Invalid-Audience";
    private const string TokenInvalidIssuerHeader = "Token-Invalid-Issuer";
    private const string TokenInvalidSignatureHeader = "Token-Invalid-Signature";
    private const string TokenNoExpirationHeader = "Token-No-Expiration";

    /// <summary>
    ///     配置Jwt
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        JwtOption? config = configuration.GetGeneric<JwtOption>("JwtOptions");
        ArgumentNullException.ThrowIfNull(config);

        services.AddSingleton(config);

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(config);
    }

    /// <summary>
    /// Adds JWT bearer authentication using the same validation rules as token issuance and refresh.
    /// </summary>
    /// <param name="builder">Authentication builder.</param>
    /// <param name="config">JWT options.</param>
    public static void AddJwtBearer(this AuthenticationBuilder builder, JwtOption config)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(config);

        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = JwtService.CreateTokenValidationParameters(
                config,
                validateLifetime: true);
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var exception = context.Exception;
                    var headerName = exception switch
                    {
                        SecurityTokenExpiredException => TokenExpiredHeader,
                        SecurityTokenInvalidSignatureException => TokenInvalidSignatureHeader,
                        SecurityTokenInvalidAudienceException => TokenInvalidAudienceHeader,
                        SecurityTokenInvalidIssuerException => TokenInvalidIssuerHeader,
                        SecurityTokenNoExpirationException => TokenNoExpirationHeader,
                        _ => null
                    };
                    if (headerName is not null)
                    {
                        context.Response.Headers[headerName] = "true";
                    }

                    var loggerFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Linger.AspNetCore.Jwt");
                    logger.LogDebug(exception, "JWT authentication failed");

                    return Task.CompletedTask;
                }
            };
        });
    }
}
