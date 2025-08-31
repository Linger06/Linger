#pragma warning disable
#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public CompilerFeatureRequiredAttribute(string feature) => FeatureName = feature;
    public string FeatureName { get; }
    public bool IsOptional { get; set; }
    public const string RefStructs = "RefStructs";
    public const string RequiredMembers = "RequiredMembers";
}

// Support for init accessors on older TFMs
internal static class IsExternalInit { }
#endif
