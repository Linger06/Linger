using System.Text.Json;
using Linger.EFCore.Comparers;
using Linger.EFCore.Converters;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore;
public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> propertyBuilder, JsonSerializerOptions? options = null) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        var converter = new ValueConverter<T?, string>
        (
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<T?>(v, options)
        );

        var comparer = new ValueComparer<T?>
        (
            (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
            v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
        );

        propertyBuilder.HasConversion(converter, comparer);
        return propertyBuilder;
    }

    public static PropertyBuilder<T?> HasStringCollectionConversion<T>(this PropertyBuilder<T?> propertyBuilder, string separator = ";") where T : class, IEnumerable<string>
    {
        var converter = new StringCollectionConverter<T>(separator);
        var comparer = new StringCollectionComparer();

        propertyBuilder.HasConversion(converter, comparer);
        return propertyBuilder;
    }
}
