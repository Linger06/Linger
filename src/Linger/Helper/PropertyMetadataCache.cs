using System.Collections.Concurrent;
using System.Reflection;

namespace Linger.Helper;

internal static class PropertyMetadataCache
{
    private static readonly ConcurrentDictionary<(Type Type, BindingFlags Flags), PropertyInfo[]> s_properties = new();
    private static readonly ConcurrentDictionary<(Type Type, BindingFlags Flags, bool IgnoreCase, bool WritableOnly), IReadOnlyDictionary<string, PropertyInfo>> s_propertyMaps = new();

    public static PropertyInfo[] GetProperties(
        Type type,
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        ArgumentNullException.ThrowIfNull(type);
        return s_properties.GetOrAdd((type, flags), static key => key.Type.GetProperties(key.Flags));
    }

    public static IReadOnlyDictionary<string, PropertyInfo> GetPropertyMap(
        Type type,
        bool ignoreCase = false,
        bool writableOnly = false,
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        ArgumentNullException.ThrowIfNull(type);
        return s_propertyMaps.GetOrAdd((type, flags, ignoreCase, writableOnly), static key =>
        {
            var comparer = key.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var map = new Dictionary<string, PropertyInfo>(comparer);

            foreach (PropertyInfo property in GetProperties(key.Type, key.Flags))
            {
                if (key.WritableOnly && (!property.CanWrite || property.SetMethod?.IsPublic != true))
                    continue;

                map[property.Name] = property;
            }

            return map;
        });
    }
}
