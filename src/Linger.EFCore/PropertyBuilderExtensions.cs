using System.Text.Json;
using Linger.EFCore.Comparers;
using Linger.EFCore.Converters;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore;

/// <summary>
/// Provides value conversion helpers for Entity Framework Core properties.
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Configures JSON storage, structural comparison, and snapshotting for a reference type.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="propertyBuilder">The property builder.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>The same property builder instance.</returns>
    public static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> propertyBuilder, JsonSerializerOptions? options = null) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
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

    /// <summary>
    /// Configures JSON storage, structural comparison, and snapshotting for a string collection.
    /// </summary>
    /// <typeparam name="TCollection">The string collection type.</typeparam>
    /// <param name="propertyBuilder">The property builder.</param>
    /// <returns>The same property builder instance.</returns>
    public static PropertyBuilder<TCollection?> HasStringCollectionConversion<TCollection>(this PropertyBuilder<TCollection?> propertyBuilder) where TCollection : class, IEnumerable<string>
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        var converter = new StringCollectionConverter<TCollection>();
        var comparer = new StringCollectionComparer<TCollection>();

        propertyBuilder.HasConversion(converter, comparer);
        return propertyBuilder;
    }
}
