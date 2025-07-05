using System.Security.Claims;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Linger.Configuration;
using Linger.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
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
        // 直接注册JwtOption实例
        JwtOption? config = configuration.GetGeneric<JwtOption>("JwtOptions");
        ArgumentNullException.ThrowIfNull(config);

        services.AddSingleton(config);

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(config);

        //services.AddScoped<IJwtService, JwtService>();
    }

    public static void AddJwtBearer(this AuthenticationBuilder builder, JwtOption config)
    {
        ArgumentNullException.ThrowIfNull(config);
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

            // 增强JWT验证
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var exception = context.Exception;
                    Console.WriteLine($"Token解析失败(如过期、签名错误): {exception.Message}");
                    if (exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    else if (exception is SecurityTokenInvalidSignatureException)
                    {
                        context.Response.Headers.Append("Token-Invalid-Signature", "true");
                    }
                    else if (exception is SecurityTokenInvalidAudienceException)
                    {
                        context.Response.Headers.Append("Token-Invalid-Audience", "true");
                    }
                    else if (exception is SecurityTokenInvalidIssuerException)
                    {
                        context.Response.Headers.Append("Token-Invalid-Issuer", "true");
                    }
                    else if (exception is SecurityTokenNoExpirationException)
                    {
                        context.Response.Headers.Append("Token-No-Expiration", "true");
                    }
                    else
                    {
                        // 添加日志记录
                        Console.WriteLine(exception.ToString(), "An unhandled exception occurred during authentication.");
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    Console.WriteLine($"Token有效但权限不足");
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsJsonAsync(new { status = 403, msg = "无权限访问" });
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"请求未携带Token或Token无效: {context.AuthenticateFailure?.Message}");

                    // 自定义处理失败挑战的响应
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    // 获取异常信息
                    var errorInfo = context.Error;
                    var errorDescription = context.ErrorDescription;

                    var result = new { error = errorInfo, error_description = errorDescription };
                    context.Response.WriteAsync(result.ToJsonString());
                    return Task.CompletedTask;
                }
            };
        });
    }
}
