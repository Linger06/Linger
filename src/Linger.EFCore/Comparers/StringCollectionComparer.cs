using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Linger.EFCore.Comparers;

/// <summary>
/// Provides structural comparison and snapshotting for string collection types.
/// </summary>
/// <typeparam name="TCollection">The collection type.</typeparam>
public sealed class StringCollectionComparer<TCollection>(JsonSerializerOptions? options = null) : ValueComparer<TCollection?>(
    (left, right) => AreEqual(left, right),
    value => GetCollectionHashCode(value),
    value => CreateSnapshot(value, options))
    where TCollection : class, IEnumerable<string>
{
    private static bool AreEqual(TCollection? left, TCollection? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        return left is not null &&
               right is not null &&
               left.SequenceEqual(right, StringComparer.Ordinal);
    }

    private static int GetCollectionHashCode(TCollection? value)
    {
        if (value is null)
        {
            return 0;
        }

        var hashCode = new HashCode();
        foreach (var item in value)
        {
            hashCode.Add(item, StringComparer.Ordinal);
        }

        return hashCode.ToHashCode();
    }

    private static TCollection? CreateSnapshot(TCollection? value, JsonSerializerOptions? options)
    {
        return value is null
            ? null
            : JsonSerializer.Deserialize<TCollection>(JsonSerializer.Serialize(value, options), options);
    }
}
