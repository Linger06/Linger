#pragma warning disable
#if !NET7_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SetsRequiredMembersAttribute : Attribute
{
}
#endif
