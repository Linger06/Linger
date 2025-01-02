using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore.Converters;

public class StringCollectionConverter<T> : ValueConverter<T?, string> where T : class, IEnumerable<string>
{
    public StringCollectionConverter(string separator = ";")
        : base(
            v => v != null ? string.Join(separator, v) : string.Empty,
            v => (T)CreateCollection(v != null ? v.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>())
        )
    {
    }

    private static IEnumerable<string> CreateCollection(string[] items)
    {
        if (typeof(T) == typeof(string[]))
            return items;
        if (typeof(T) == typeof(List<string>))
            return items.ToList();
        if (typeof(T) == typeof(ICollection<string>))
            return items.ToList();
        if (typeof(T) == typeof(IEnumerable<string>))
            return items;

        throw new ArgumentException($"Unsupported collection type: {typeof(T)}");
    }
}
