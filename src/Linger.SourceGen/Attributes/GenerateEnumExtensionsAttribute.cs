namespace Linger.SourceGen;

/// <summary>
/// 标注在枚举类型上，生成 Name/Value/显示文本 映射与无反射的解析与扩展方法。
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
internal sealed class GenerateEnumExtensionsAttribute : System.Attribute
{
}
