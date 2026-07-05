using System.Text;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// 当字符串为 null 时，返回指定的默认字符串（最高效的直接赋值）。
    /// </summary>
    public static string ToStringOrDefault(this string? value, string defaultValue = "")
    {
        return value ?? defaultValue;
    }

    /// <summary>
    /// 当字符串为 null 时，通过延迟加载函数返回默认字符串（适用于高开销的默认值生成）。
    /// </summary>
    public static string ToStringOrDefault(this string? value, Func<string> defaultValueFunc)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFunc);
        return value ?? defaultValueFunc();
    }

    /// <summary>
    /// 针对“空字符串”或“空白字符”的更严格清洗工具。
    /// </summary>
    public static string ToSafeString(this string? value, string defaultValue = "", bool allowEmpty = false)
    {
        if (allowEmpty)
        {
            return value ?? defaultValue;
        }

        // 使用 .IsNullOrWhiteSpace 处理 null、"" 和 "   "
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    #region  string? -> short

    public static short ToShort(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture).ToShort();
    }

    public static short? ToShortOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decResult))
            return decResult.ToShortOrNull();

        return null;
    }

    public static short ToShortOrDefault(this string? value, short defaultValue = 0)
        => value.ToShortOrNull() ?? defaultValue;

    public static bool TryToShort(this string? value, out short result)
    {
        var r = value.ToShortOrNull();
        result = r ?? default;
        return r.HasValue;
    }
    #endregion

    #region string? -> Guid

    /// <summary>
    /// 【严格转换派】将字符串严格解析为 Guid。
    /// 失败（格式错误、长度不对、null）时严格且立刻抛出底层的 FormatException。
    /// </summary>
    public static Guid ToGuid(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.ToGuidOrNull() ?? throw new FormatException($"The string '{value}' is not a valid Guid format.");
    }

    /// <summary>
    /// 【可空宽容派】智能兼容各类标准 Guid 格式（带/不带连字符、大括号等）。
    /// 全网绝对不抛出任何异常，转不了优雅返回 null。
    /// </summary>
    public static Guid? ToGuidOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        // 依靠原生强大的内置解析器，自动识别 N/D/B/P/X 五种标准文本表现形式
        if (Guid.TryParse(trimmed, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static Guid ToGuidOrDefault(this string? value, Guid defaultValue = default)
        => value.ToGuidOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】完美对齐 .NET 原生 TryParse 语法（out Guid 强类型输出）。
    /// </summary>
    public static bool TryToGuid(this string? value, out Guid result)
    {
        var r = value.ToGuidOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region  string? -> int

    /// <summary>
    /// 【严格转换派】将字符串严格转换为整数。
    /// 格式错误、含非预期小数、数值越界、或传入 null 时严格且立刻抛出精准的底层异常。
    /// </summary>
    public static int ToInt(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // 1. 极速纯整型路径（无符号、无千分位、无小数点的标准整型文本）
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        // 2. 失败时流向财务线 decimal：支持类似 "42.0" 的特殊文本，并借助您之前漂亮的 decimal.ToInt() 执行严格小数/越界校验
        return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture).ToInt();
    }

    /// <summary>
    /// 【可空宽容派】纯净。能转就转，转不了（含溢出或非预期小数）直接返回 null，全网绝不抛错。
    /// </summary>
    public static int? ToIntOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        // 智能兼容：处理像 "42.0" 或带财务千分位的整数，并交由您硬核的 decimal.ToIntOrNull() 质检
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decResult))
            return decResult.ToIntOrNull();

        return null;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗：失败直接返回指定的固定默认值。
    /// </summary>
    public static int ToIntOrDefault(this string? value, int defaultValue = 0)
        => value.ToIntOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗：完美对齐 .NET 标准常规控流语法（out int 强类型输出）。
    /// </summary>
    public static bool TryToInt(this string? value, out int result)
    {
        var r = value.ToIntOrNull();
        result = r ?? default;
        return r.HasValue;
    }
    #endregion

    #region  string? -> long

    public static long ToLong(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture).ToLong();
    }

    public static long? ToLongOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decResult))
            return decResult.ToLongOrNull();

        return null;
    }

    public static long ToLongOrDefault(this string? value, long defaultValue = 0L)
        => value.ToLongOrNull() ?? defaultValue;

    public static bool TryToLong(this string? value, out long result)
    {
        var r = value.ToLongOrNull();
        result = r ?? default;
        return r.HasValue;
    }
    #endregion

    #region string? -> decimal

    /// <summary>
    /// 【严格转换派】将字符串严格转换为 decimal。失败时抛出最精准的 FormatException 或 OverflowException。
    /// </summary>
    public static decimal ToDecimal(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // 锁死 NumberStyles.Any（天然支持千分位逗号 "1,234.56" 和科学计数法）以及不区分文化的独立标准
        return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 【可空宽容派】财务高精度文本解析终点。全网绝对不抛出任何异常。
    /// </summary>
    public static decimal? ToDecimalOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static decimal ToDecimalOrDefault(this string? value, decimal defaultValue = default)
        => value.ToDecimalOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】完美对齐 .NET 标准的 TryParse 语法（out decimal 强类型输出）。
    /// </summary>
    public static bool TryToDecimal(this string? value, out decimal result)
    {
        var r = value.ToDecimalOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region string? -> DateTime

    /// <summary>
    /// 【严格转换派】将字符串严格解析为 DateTime。
    /// 锁死 InvariantCulture 与 RoundtripKind，失败时严格抛出底层的 FormatException。
    /// </summary>
    public static DateTime ToDateTime(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.ToDateTimeOrNull() ?? throw new FormatException($"The string '{value}' is not a valid Invariant Culture or ISO 8601 DateTime format.");
    }

    /// <summary>
    /// 【可空宽容派】时区安全解析大漏斗终点。
    /// 全网绝对不抛出任何异常，转不了优雅返回 null。
    /// </summary>
    public static DateTime? ToDateTimeOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        // 策略 A：优先尝试 ISO 8601 往返格式（含 Z、时区偏移等），并锁死不区分文化的独立标准
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            return result;
        }

        // 策略 B：次级尝试允许带前导/后导空格的常规不区分文化解析（如 "2026/07/03 12:00:00"）
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static DateTime ToDateTimeOrDefault(this string? value, DateTime defaultValue = default)
        => value.ToDateTimeOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】完美对齐 .NET 原生 TryParse 语法（out DateTime 强类型输出）。
    /// </summary>
    public static bool TryToDateTime(this string? value, out DateTime result)
    {
        var r = value.ToDateTimeOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region string? -> bool

    private static readonly Dictionary<string, bool> s_boolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0", false }, { "false", false }, { "no", false }, { "n", false }, { "fail", false }, { "lose", false }, { "off", false },
        { "1", true }, { "true", true }, { "yes", true }, { "y", true }, { "success", true }, { "ok", true }, { "on", true }
    };

#if NET6_0_OR_GREATER
    private static bool TryParseBoolExtended(ReadOnlySpan<char> value, out bool result)
    {
        var trimmed = value.Trim();
        // 兼容原生的 bool.TryParse 高性能解析
        if (bool.TryParse(trimmed, out result)) return true;
        // 匹配自定义智能字典
        return s_boolMap.TryGetValue(trimmed.ToString(), out result);
    }
#else
    private static bool TryParseBoolExtended(string value, out bool result)
    {
        var trimmed = value.Trim();
        if (bool.TryParse(trimmed, out result)) return true;
        return s_boolMap.TryGetValue(trimmed, out result);
    }
#endif

    /// <summary>
    /// 【严格转换派】将字符串严格转换为布尔值。失败时抛出最精准的 InvalidCastException。
    /// </summary>
    public static bool ToBool(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.ToBoolOrNull() ?? throw new InvalidCastException($"The value '{value}' cannot be converted to Boolean.");
    }

    /// <summary>
    /// 【可空宽容派】智能识别文本核心链。全网绝对不抛出任何异常。
    /// </summary>
    public static bool? ToBoolOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

#if NET6_0_OR_GREATER
        if (TryParseBoolExtended(value.AsSpan(), out var result))
            return result;
#else
        if (TryParseBoolExtended(value, out var result))
            return result;
#endif

        return null;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static bool ToBoolOrDefault(this string? value, bool defaultValue = default)
        => value.ToBoolOrNull() ?? defaultValue;

    /// <summary>
    /// 【固定默认值派 - 委托重载】单行漏斗。
    /// </summary>
    public static bool ToBoolOrDefault(this string? value, Func<bool>? defaultValueFunc)
        => value.ToBoolOrNull() ?? defaultValueFunc?.Invoke() ?? default;

    /// <summary>
    /// 【标准布尔控流派】完美对齐 .NET 原生 TryParse 语法（out bool 强类型输出）。
    /// </summary>
    public static bool TryToBool(this string? value, out bool result)
    {
        var r = value.ToBoolOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region Bytes

   /// <summary>
    /// 尝试将字符串转换为字节数组（UTF-8 编码）。
    /// </summary>
    public static bool TryToBytes(this string? value, [NotNullWhen(true)] out byte[]? result)
        => value.TryToBytes(Encoding.UTF8, out result);

    /// <summary>
    /// 尝试将字符串转换为字节数组（指定编码）。
    /// </summary>
    public static bool TryToBytes(this string? value, Encoding encoding, [NotNullWhen(true)] out byte[]? result)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        if (string.IsNullOrEmpty(value)) // 放行空格字符，仅拦截真正的 null 和 ""
        {
            result = null;
            return false;
        }

        result = encoding.GetBytes(value);
        return true;
    }

    /// <summary>
    /// 将字符串转换为字节数组。若失败或输入无效，返回指定的直接默认值（默认值为 null）。
    /// </summary>
    /// <param name="value">要转换的字符串。</param>
    /// <param name="defaultValue">转换失败时返回的直接字节数组，默认为 null。</param>
    /// <param name="encoding">字符编码，默认为 UTF-8。</param>
    public static byte[]? ToBytesOrNull(this string? value, byte[]? defaultValue = null, Encoding? encoding = null)
    {
        return value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// 将字符串转换为字节数组。仅当默认值计算逻辑非常沉重时，才走这个延迟加载委托版本。
    /// </summary>
    /// <param name="value">要转换的字符串。</param>
    /// <param name="defaultValueFunc">用于生成默认值的延迟加载函数。</param>
    /// <param name="encoding">字符编码，默认为 UTF-8。</param>
    public static byte[]? ToBytesOrNull(this string? value, Func<byte[]?> defaultValueFunc, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFunc);
        return value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : defaultValueFunc();
    }

    /// <summary>
    /// 将字符串转换为字节数组。若字符串为 null 或空，返回空数组 []（C# 12+ 集合表达式最快语法）。
    /// </summary>
    /// <param name="value">要转换的字符串。</param>
    /// <param name="encoding">字符编码，默认为 UTF-8。</param>
    public static byte[] ToBytes(this string? value, Encoding? encoding = null)
    {
        return value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : [];
    }

    #endregion

    #region Stream

  /// <summary>
    /// 将字符串转换为内存流。如果字符串为空，返回不可写的 Stream.Null。
    /// </summary>
    /// <param name="value">要转换的字符串。</param>
    /// <param name="encoding">字符编码，默认为 UTF-8。</param>
    public static Stream ToStream(this string? value, Encoding? encoding = null)
        => value.ToStreamOrNull(encoding) ?? Stream.Null;

    /// <summary>
    /// 将字符串转换为只读内存流视图（带可选编码支持）。
    /// </summary>
    /// <param name="value">要转换的字符串。</param>
    /// <param name="encoding">字符编码，默认为 UTF-8。</param>
    public static Stream? ToStreamOrNull(this string? value, Encoding? encoding = null)
    {
        // 保持对空格字符的放行（符合数据流逻辑），仅拦截真正的 null 和 ""
        if (string.IsNullOrEmpty(value)) return null;

        // 复用精简后的 ToBytes
        var bytes = value.ToBytes(encoding ?? Encoding.UTF8);

        // 指定 writable: false，从底层锁死只读，防止外部错误写入导致流缓冲区扩容
        return new MemoryStream(bytes, writable: false);
    }
    #endregion
}
