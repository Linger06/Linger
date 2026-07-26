using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore.Converters;

/// <summary>
/// Converts string collection types to and from JSON.
/// </summary>
/// <typeparam name="TCollection">The collection type.</typeparam>
public sealed class StringCollectionConverter<TCollection> : ValueConverter<TCollection?, string>
    where TCollection : class, IEnumerable<string>
{
    /// <summary>
    /// Initializes a new converter.
    /// </summary>
    /// <param name="options">Optional JSON serializer options.</param>
    public StringCollectionConverter(JsonSerializerOptions? options = null)
        : base(
            value => JsonSerializer.Serialize(value, options),
            value => JsonSerializer.Deserialize<TCollection>(value, options))
    {
    }
}
