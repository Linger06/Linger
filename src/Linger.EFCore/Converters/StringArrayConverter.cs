using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Linger.EFCore.Converters;
public class StringArrayConverter : ValueConverter<ICollection<string>?, string?>
{
    public StringArrayConverter(string separator = ";") : base(
            convertToProviderExpression: v => v.ToSeparatedString(separator, null),
            convertFromProviderExpression: v => v.IsNotNullAndEmpty() ? v.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : Array.Empty<string>())
    {
    }
}
