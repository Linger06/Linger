using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

public static class IConfigurationExtensions
{
    /// <summary>
    ///     获取节点的配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static T? GetGeneric<T>(this IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ArgumentNullException.ThrowIfNull(sectionName);

        IConfigurationSection section = configuration.GetSection(sectionName);

        T? value = section.Get<T>();
        return value;
    }
}