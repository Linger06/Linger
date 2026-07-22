using System.Reflection;

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides reflection-based extension methods for enumerable metadata.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime custom attribute metadata. This API is not compatible with trimming.")]
#endif
public static class EnumerableReflectionExtensions
{
#if !NETFRAMEWORK || NET462_OR_GREATER
    /// <summary>
    /// Determines whether custom attribute metadata contains the specified attribute type.
    /// </summary>
    /// <param name="customAttributes">The source custom attribute metadata.</param>
    /// <param name="attributeType">The type of attribute to find.</param>
    /// <returns><see langword="true"/> when an attribute of the requested type exists; otherwise, <see langword="false"/>.</returns>
    public static bool HasAttribute(this IEnumerable<CustomAttributeData> customAttributes, Type attributeType)
    {
        ArgumentNullException.ThrowIfNull(customAttributes);
        ArgumentNullException.ThrowIfNull(attributeType);

        return customAttributes.Any(attribute => attribute.AttributeType == attributeType);
    }
#endif
}
