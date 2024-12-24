#if NETSTANDARD2_0 || NETFRAMEWORK

// These Annotations are Part of .NET Standard 2.1 and .NET Core 3.0+. Defining them here allows
// their usage to support developers using this library with nullable-reference-type-warnings enabled.
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}

#endif

#if NETSTANDARD1_0 || NETSTANDARD2_0 || NETFRAMEWORK
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Required for reference nullability annotations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.
        /// </summary>
        /// <param name="returnValue">
        /// If the method returns this value, the associated parameter will not be null.
        /// </param>
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>
        /// Gets the return value condition.
        /// </summary>
        public bool ReturnValue { get; }
    }

    [AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
    AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Event |
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.GenericParameter)]
    internal sealed class NotNullAttribute : Attribute { }
}
#endif