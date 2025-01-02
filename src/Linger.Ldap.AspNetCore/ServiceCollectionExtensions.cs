
using System.Runtime.Versioning;
using Linger.Helper;
using Linger.Ldap.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linger.Ldap.AspNetCore;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static class ServiceCollectionExtensions
{
    public static void AddLdapService(this IServiceCollection services, IConfiguration configuration)
    {
        var ldapConfig = configuration.GetSection("LdapConfig").Get<LdapConfig>();
        ldapConfig.EnsureIsNotNull();
        services.AddSingleton(ldapConfig);
        services.AddTransient<ILdapService, LdapService>();
        services.AddTransient<ILdap, ActiveDirectory.Ldap>();
    }
}