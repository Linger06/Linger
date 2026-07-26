using Linger.EFCore.Comparers;
using Linger.EFCore.Converters;

namespace Linger.EFCore.UnitTests;

public class StringCollectionConversionTests
{
    [Fact]
    public void Converter_RoundTripsSeparatorAndEmptyValues()
    {
        var converter = new StringCollectionConverter<List<string>>();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();
        var expected = new List<string> { "a;b", string.Empty, "c|d", "e,f" };

        var providerValue = toProvider(expected);
        var result = fromProvider(providerValue);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Converter_RoundTripsCollectionInterfaces()
    {
        var expected = new[] { "one", "two" };
        var enumerableConverter = new StringCollectionConverter<IEnumerable<string>>();
        var collectionConverter = new StringCollectionConverter<ICollection<string>>();

        var enumerable = enumerableConverter.ConvertFromProviderExpression.Compile()(
            enumerableConverter.ConvertToProviderExpression.Compile()(expected));
        var collection = collectionConverter.ConvertFromProviderExpression.Compile()(
            collectionConverter.ConvertToProviderExpression.Compile()(expected));

        Assert.Equal(expected, enumerable);
        Assert.Equal(expected, collection);
    }

    [Fact]
    public void Comparer_UsesStructuralHashCode()
    {
        var comparer = new StringCollectionComparer<List<string>>();
        var left = new List<string> { "one", "two" };
        var right = new List<string> { "one", "two" };

        Assert.True(comparer.Equals(left, right));
        Assert.Equal(comparer.GetHashCode(left), comparer.GetHashCode(right));
    }

    [Fact]
    public void Comparer_SnapshotDetectsInPlaceMutation()
    {
        var comparer = new StringCollectionComparer<List<string>>();
        var value = new List<string> { "one", "two" };
        var snapshot = Assert.IsType<List<string>>(comparer.Snapshot(value));

        value.Add("three");

        Assert.False(comparer.Equals(value, snapshot));
        Assert.Equal(new[] { "one", "two" }, snapshot);
    }
}
