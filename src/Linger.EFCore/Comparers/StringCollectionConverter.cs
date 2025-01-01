using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore.Converters
{
    public class StringCollectionConverter<T> : ValueConverter<T, string> where T : class, IEnumerable<string>
    {
        public StringCollectionConverter(string separator = ";")
            : base(
                // 转换到数据库
                v => v != null ? string.Join(separator, v) : string.Empty,
                // 从数据库转换回来
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
}
