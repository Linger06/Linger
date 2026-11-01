#pragma warning disable
#if !NET7_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class SetsRequiredMembersAttribute : Attribute
{
}
#endif
