// Moved from Polyfills/BCL/RequiredMemberAttribute.cs
#pragma warning disable
#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class RequiredMemberAttribute : Attribute
{
}
#endif
