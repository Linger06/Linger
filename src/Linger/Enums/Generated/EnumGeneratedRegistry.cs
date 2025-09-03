using System.Collections.Concurrent;

namespace Linger.Enums;

/// <summary>
/// 由 Source Generator 进行注册的枚举快速路径提供者注册表。
/// 运行时调用方可优先走注册的委托；未注册则回退到反射/标准实现。
/// </summary>
internal static class EnumGeneratedRegistry
{
    internal sealed class Provider
    {
        public required Func<string, (bool Success, object? Value)> TryGetByName { get; init; }
        public required Func<int, (bool Success, object? Value)> TryGetByInt { get; init; }
        public required Func<object, string> GetName { get; init; }
        public required Func<object, string> GetDisplayName { get; init; }
    }

    private static readonly ConcurrentDictionary<Type, Provider> s_providers = new();

    public static void Register<TEnum>(Provider provider) where TEnum : struct, Enum
        => s_providers[typeof(TEnum)] = provider;

    public static bool TryGetProvider(Type enumType, out Provider provider)
        => s_providers.TryGetValue(enumType, out provider!);
}
