#pragma warning disable
// Provide shim only for targets that don't include CallerArgumentExpressionAttribute (.NET 6+ has it in System.Runtime.CompilerServices)
#if NETSTANDARD2_0 || NET462 || NET472 || NETFRAMEWORK
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;
    public string ParameterName { get; }
}
#endif