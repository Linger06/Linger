using System.Reflection;
using Linger.Extensions.Collection;

namespace Linger.UnitTests.Extensions.Collection;

public class EnumerableReflectionExtensionsTests
{
#if !NETFRAMEWORK || NET462_OR_GREATER
    [Fact]
    public void HasAttribute_WhenTypeContainsAttribute_ReturnsTrue()
    {
        IList<CustomAttributeData> attributes = typeof(DecoratedType).GetCustomAttributesData();

        bool result = attributes.HasAttribute(typeof(SampleAttribute));

        Assert.True(result);
    }

    [Sample]
    private sealed class DecoratedType
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class SampleAttribute : Attribute
    {
    }
#endif
}
