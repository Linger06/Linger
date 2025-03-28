using System.Security.Claims;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Linger.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     配置Jwt
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        // 使用IOptions配置
        services.Configure<JwtOption>(configuration.GetSection("JwtOptions"));
        JwtOption? config = configuration.GetGeneric<JwtOption>("JwtOptions");
        ArgumentNullException.ThrowIfNull(config);

        services.AddSingleton(config);

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
                //是否验证发行人，就是验证载荷中的Iss是否对应ValidIssuer参数
                ValidateIssuer = true,
                //发行人
                ValidIssuer = config.Issuer,
                //是否验证订阅人，就是验证载荷中的Aud是否对应ValidAudience参数
                ValidateAudience = true,
                //订阅人
                ValidAudience = config.Audience,
                //是否验证签名,不验证的话可以篡改数据，不安全
                ValidateIssuerSigningKey = true,
                //解密的密钥
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecurityKey)),
                //是否验证过期时间,过期了就拒绝访问,使用当前时间与Token的Claims中的NotBefore和Expires对比.同时启用ClockSkew
                ValidateLifetime = true,
                //总的Token有效时间 = JwtRegisteredClaimNames.Exp + ClockSkew ；
                RequireExpirationTime = true,
                //注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"认证失败: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"令牌验证成功: {context.SecurityToken}");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"认证挑战: {context.AuthenticateFailure?.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        //services.AddScoped<IJwtService, JwtService>();
    }
}