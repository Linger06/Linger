namespace Linger.SourceGen;

/// <summary>
/// 标注在枚举类型上，生成 Name/Value/显示文本映射与无反射解析扩展。
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class GenerateEnumExtensionsAttribute : System.Attribute
{
}
