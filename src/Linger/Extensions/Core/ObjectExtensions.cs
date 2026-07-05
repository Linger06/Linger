using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linger.Helper;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="object"/> extensions
/// </summary>
public static class ObjectExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> s_propertyCache = new();

    /// <summary>
    /// 获取指定属性名称的 PropertyInfo（带高效线程安全缓存）。
    /// </summary>
    public static PropertyInfo? GetPropertyInfo(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);

        var type = obj.GetType();

        // 优化点 1：将复杂的字典构建逻辑抽离成独立的静态方法，避免多层 lambda 套娃和高并发重复执行隐患
        var map = s_propertyCache.GetOrAdd(type, CreatePropertyMap);

        return map.TryGetValue(propertyName, out var pi) ? pi : null;
    }

    /// <summary>
    /// 获取指定属性的值。如果属性不存在，返回 null（更安全的工程实践）。
    /// </summary>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var pi = obj.GetPropertyInfo(propertyName);

        // 优化点 2：优雅降级，属性不存在时返回 null，而不是粗暴地抛出 InvalidOperationException 导致进程崩溃
        return pi?.GetValue(obj, null);
    }

    // 核心优化 3：独立的字典工厂，确保反射操作在最外层一步到位完成
    private static Dictionary<string, PropertyInfo> CreatePropertyMap(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dict = new Dictionary<string, PropertyInfo>(props.Length, StringComparer.Ordinal);

        foreach (var p in props)
        {
            // 考虑重名覆盖问题（虽然标准属性没有，但继承链上可能有 new 关键字修饰的属性）
            dict[p.Name] = p;
        }

        return dict;
    }

    /// <summary>
    /// 指示指定的对象是否为 null。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    /// <summary>
    /// 指示指定的对象是否不为 null。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    /// <summary>
    /// 指示指定的对象是否为 null，或者其字符串表示形式/内容是否为空。
    /// </summary>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value)
    {
        if (value is null) return true;
        if (value is string str) return string.IsNullOrEmpty(str); // ⚡ 核心优化1：如果是字符串，走最高效的原生通道

        // ⚡ 核心优化2：处理那些重度依赖重写 ToString 的特殊对象
        var objectStr = value.ToString();
        return string.IsNullOrEmpty(objectStr);
    }

    /// <summary>
    /// 指示指定的对象是否不为 null，且其字符串表示形式/内容不为空。
    /// </summary>
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this object? value)
    {
        if (value is null) return false;
        if (value is string str) return !string.IsNullOrEmpty(str); // ⚡ 同上：避免字符串类型被二次装箱和无端调用 ToString()

        var objectStr = value.ToString();
        return !string.IsNullOrEmpty(objectStr);
    }

    /// <summary>
    /// 指示指定的对象是 null 还是 <see cref="DBNull"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrDbNull([NotNullWhen(false)] this object? value) => value is DBNull or null;

    /// <summary>
    /// 判断对象是否为任一数值类型（包含所有内置整型、浮点型和 decimal）。
    /// </summary>
    public static bool IsNumeric(this object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    /// <summary>
    /// 将对象转换为标准化字符串，支持裁剪和空值/空白字符的统归。
    /// </summary>
    public static string? ToNormalizedString(this object? input, bool trim = false, bool treatEmptyAsNull = false)
    {
        if (input == null) return null;

        var result = input.ToString();

        if (trim)
        {
            result = result?.Trim();
        }

        // 优化点：既然支持 treatEmptyAsNull，如果用户进行了 trim，这里建议升级为 IsNullOrWhiteSpace，防范全面
        if (treatEmptyAsNull && string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        return string.IsNullOrEmpty(result) && treatEmptyAsNull ? null : result;
    }

    /// <summary>
    /// 将输入对象转换为已裁切前后空格的字符串。如果输入为 null，则返回空字符串。
    /// </summary>
    public static string ToTrimmedString(this object? input)
    {
        // 复用底层核心逻辑，避免逻辑散落
        return input.ToNormalizedString(trim: true) ?? string.Empty;
    }

    /// <summary>
    /// 将输入对象转换为字符串。如果输入为 null，返回指定的默认值。
    /// </summary>
    public static string ToStringOrDefault(this object? input, string defaultValue = "")
    {
        return input?.ToString() ?? defaultValue; // 保持原本的高效指针判空，不经过复杂清洗
    }

    #region object? -> short

    public static short ToShort(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            short s => s,
            int i => checked((short)i),                 // 拦截 int 转 short 溢出
            long l => checked((short)l),                 // 拦截 long 转 short 溢出
            decimal dec => dec.ToShort(),               // 直达您手写的 decimal.ToShort() 质检员
            string str => str.ToShort(),
            double d => ((decimal)d).ToShort(),
            float f => ((decimal)f).ToShort(),
            _ => value.ToString().ToShort()
        };
    }

    public static short? ToShortOrNull(this object? value)
    {
        if (value is null || value is DBNull) return null;

        return value switch
        {
            short s => s,
            int i => (i >= short.MinValue && i <= short.MaxValue) ? (short)i : null, // 内存快速范围拦截
            long l => (l >= short.MinValue && l <= short.MaxValue) ? (short)l : null,
            decimal dec => dec.ToShortOrNull(),         // 直达您手写的 decimal.ToShortOrNull() 质检员
            string str => str.ToShortOrNull(),
            double d => decimal.TryParse(d.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToShortOrNull() : null,
            float f => decimal.TryParse(f.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToShortOrNull() : null,
            _ => value.ToString().ToShortOrNull()
        };
    }

    public static short ToShortOrDefault(this object? value, short defaultValue = 0) => value.ToShortOrNull() ?? defaultValue;
    public static bool TryToShort(this object? value, out short result)
    {
        var r = value.ToShortOrNull();
        result = r ?? default;
        return r.HasValue;
    }
    #endregion

    #region object? -> long

    public static long ToLong(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            long l => l,
            int i => i,
            short s => s,
            decimal dec => dec.ToLong(),               // 直达您手写的 decimal.ToLong() 质检员
            string str => str.ToLong(),
            double d => ((decimal)d).ToLong(),
            float f => ((decimal)f).ToLong(),
            _ => value.ToString().ToLong()
        };
    }

    public static long? ToLongOrNull(this object? value)
    {
        if (value is null || value is DBNull) return null;

        return value switch
        {
            long l => l,
            int i => i,
            short s => s,
            decimal dec => dec.ToLongOrNull(),         // 直达您手写的 decimal.ToLongOrNull() 质检员
            string str => str.ToLongOrNull(),
            double d => decimal.TryParse(d.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToLongOrNull() : null,
            float f => decimal.TryParse(f.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToLongOrNull() : null,
            _ => value.ToString().ToLongOrNull()
        };
    }

    public static long ToLongOrDefault(this object? value, long defaultValue = 0L)
        => value.ToLongOrNull() ?? defaultValue;

    public static bool TryToLong(this object? value, out long result)
    {
        var r = value.ToLongOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region object? -> decimal

    /// <summary>
    /// 【严格转换派】利用模式匹配（Pattern Matching）对高频数值对象执行零分配、绝对无损的强拆箱。
    /// </summary>
    public static decimal ToDecimal(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            decimal dec => dec,                                // 原生就是高精度，原地直接放行
            string str => str.ToDecimal(),                     // 文本状态，转发给文本财务严格链（支持千分位）
            int i => i,                                        // 以下整数转型至 decimal 绝对无损，安全升级
            long l => l,
            short s => s,
            double d => (decimal)d,                            // 浮点数强转（若超出 decimal 范围原生会爆 OverflowException）
            float f => (decimal)f,
            _ => value.ToString().ToDecimal()                  // 其余未知对象（如某些自定义金额包装类），走文本严格兜底
        };
    }

    /// <summary>
    /// 【可空宽容派】object? 金融分流大漏斗，完美兼容数据库中的 DBNull 财务空字段。
    /// </summary>
    public static decimal? ToDecimalOrNull(this object? value)
    {
        if (value is null || value is DBNull)
            return null;

        return value switch
        {
            decimal dec => dec,
            string str => str.ToDecimalOrNull(),               // 分流给文本财务宽容链
            int i => i,
            long l => l,
            short s => s,
            double d => (d >= (double)decimal.MinValue && d <= (double)decimal.MaxValue) ? (decimal)d : null, // 跨边界安全质检
            float f => (f >= (float)decimal.MinValue && f <= (float)decimal.MaxValue) ? (decimal)f : null,
            _ => value.ToString().ToDecimalOrNull()            // 最后的文本化宽容尝试
        };
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static decimal ToDecimalOrDefault(this object? value, decimal defaultValue = default)
        => value.ToDecimalOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗（out decimal 强类型输出）。
    /// </summary>
    public static bool TryToDecimal(this object? value, out decimal result)
    {
        var r = value.ToDecimalOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region object? -> int

    /// <summary>
    /// 【严格转换派】针对 object? 顶层的原生模式匹配，零垃圾（Zero-GC）极速流转分流。
    /// </summary>
    public static int ToInt(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            int i => i,
            decimal dec => dec.ToInt(),               // 直达您手写的 decimal 严格质检员（严控小数与值域）
            string str => str.ToInt(),                 // 转发给文本严格链
            long l => checked((int)l),                 // 极速拆箱，超限时通过 checked 强力炸出 OverflowException
            short s => s,                              // 安全升级
            double d => ((decimal)d).ToInt(),         // 借助 decimal 大漏斗进行严格小数与边界审查
            float f => ((decimal)f).ToInt(),
            _ => value.ToString().ToInt()             // 最后的文本化严格兜底
        };
    }

    /// <summary>
    /// 【可空宽容派】object? 大漏斗分流枢纽，全面兼容数据库 DBNull。
    /// </summary>
    public static int? ToIntOrNull(this object? value)
    {
        if (value is null || value is DBNull) return null;

        return value switch
        {
            int i => i,
            decimal dec => dec.ToIntOrNull(),         // 直达您手写的 decimal 宽容质检员
            string str => str.ToIntOrNull(),           // 转发给文本宽容链
            long l => (l >= int.MinValue && l <= int.MaxValue) ? (int)l : null, // 内存中做安全值域硬核拦截
            short s => s,
            double d => decimal.TryParse(d.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToIntOrNull() : null,
            float f => decimal.TryParse(f.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec.ToIntOrNull() : null,
            _ => value.ToString().ToIntOrNull()        // 最后的文本化宽容尝试
        };
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static int ToIntOrDefault(this object? value, int defaultValue = 0)
        => value.ToIntOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗（out int 强类型输出）。
    /// </summary>
    public static bool TryToInt(this object? value, out int result)
    {
        var r = value.ToIntOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region object? -> DateTime

    /// <summary>
    /// 【严格转换派】利用模式匹配（Pattern Matching）对高频时间对象执行零分配强拆箱。
    /// </summary>
    public static DateTime ToDateTime(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            DateTime dt => dt,                                          // 已经是 DateTime，直接返回
            DateTimeOffset dto => dto.DateTime,                         // 高级时间结构，安全提取 DateTime 部分
            string str => str.ToDateTime(),                             // 文本状态，转发给文本严格链

#if NET6_0_OR_GREATER
            DateOnly dOnly => dOnly.ToDateTime(TimeOnly.MinValue),      // .NET 6+ 高效兼容：纯日期转为当日凌晨
#endif

            _ => value.ToString().ToDateTime()                          // 最后的严格兜底
        };
    }

    /// <summary>
    /// 【可空宽容派】object? 大漏斗分流枢纽，全面兼容数据库 DBNull。
    /// </summary>
    public static DateTime? ToDateTimeOrNull(this object? value)
    {
        if (value is null || value is DBNull)
            return null;

        return value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            string str => str.ToDateTimeOrNull(),                       // 分流给文本宽容链

#if NET6_0_OR_GREATER
            DateOnly dOnly => dOnly.ToDateTime(TimeOnly.MinValue),
#endif

            _ => value.ToString().ToDateTimeOrNull()                    // 最后的文本化宽容尝试
        };
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static DateTime ToDateTimeOrDefault(this object? value, DateTime defaultValue = default)
        => value.ToDateTimeOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗（out DateTime 强类型输出）。
    /// </summary>
    public static bool TryToDateTime(this object? value, out DateTime result)
    {
        var r = value.ToDateTimeOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region object? -> bool

    /// <summary>
    /// 【严格转换派】针对 object? 顶层的原生极速拆箱与路由转发。
    /// </summary>
    public static bool ToBool(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            bool b => b,                                  // 原生布尔，直接解包返回
            string str => str.ToBool(),                   // 文本状态，转发给文本严格链
            int i => i switch                             // 常见的整数标志位，严格校验 1 和 0
            {
                1 => true,
                0 => false,
                _ => throw new InvalidCastException($"The integer value '{i}' is invalid for Boolean conversion. Only 1 and 0 are allowed.")
            },
            decimal dec => dec switch                     // 兼容可能从复杂数据源解析出来的 decimal 标志位
            {
                1m => true,
                0m => false,
                _ => throw new InvalidCastException($"The decimal value '{dec}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.")
            },
            long l => l switch
            {
                1L => true,
                0L => false,
                _ => throw new InvalidCastException($"The long value '{l}' is invalid for Boolean conversion.")
            },
            _ => value.ToString().ToBool()                // 其余未知对象，走文本严格兜底
        };
    }

    /// <summary>
    /// 【可空宽容派】object? 大漏斗分流枢纽，支持数据库 DBNull。
    /// </summary>
    public static bool? ToBoolOrNull(this object? value)
    {
        if (value is null || value is DBNull)
            return null;

        return value switch
        {
            bool b => b,
            string str => str.ToBoolOrNull(),             // 分流给文本宽容链
            int i => i switch
            {
                1 => true,
                0 => false,
                _ => null
            },
            decimal dec => dec switch
            {
                1m => true,
                0m => false,
                _ => null
            },
            long l => l switch
            {
                1L => true,
                0L => false,
                _ => null
            },
            _ => value.ToString().ToBoolOrNull()          // 最后的文本化宽容尝试
        };
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static bool ToBoolOrDefault(this object? value, bool defaultValue = default)
        => value.ToBoolOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗（out bool 强类型输出）。
    /// </summary>
    public static bool TryToBool(this object? value, out bool result)
    {
        var r = value.ToBoolOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    #region object? -> Guid

    /// <summary>
    /// 【严格转换派】针对 object? 顶层的原生极速拆箱与二进制转换路由。
    /// </summary>
    public static Guid ToGuid(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            Guid guid => guid,                                  // 原生就是 Guid，原地直通车放行
            string str => str.ToGuid(),                         // 文本状态，转发给文本严格链
            byte[] bytes when bytes.Length == 16 => new Guid(bytes), // 完美支持 16 字节底层数据库二进制行版本转换
            _ => value.ToString().ToGuid()                      // 其余未知类型，走文本严格兜底
        };
    }

    /// <summary>
    /// 【可空宽容派】object? 大漏斗分流枢纽，全面兼容数据库 DBNull。
    /// </summary>
    public static Guid? ToGuidOrNull(this object? value)
    {
        if (value is null || value is DBNull)
            return null;

        return value switch
        {
            Guid guid => guid,
            string str => str.ToGuidOrNull(),                   // 分流给文本宽容链
            byte[] bytes when bytes.Length == 16 => new Guid(bytes), // 内存无感强转
            _ => value.ToString().ToGuidOrNull()                // 最后的文本化宽容尝试
        };
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗。
    /// </summary>
    public static Guid ToGuidOrDefault(this object? value, Guid defaultValue = default)
        => value.ToGuidOrNull() ?? defaultValue;

    /// <summary>
    /// 【标准布尔控流派】单行漏斗（out Guid 强类型输出）。
    /// </summary>
    public static bool TryToGuid(this object? value, out Guid result)
    {
        var r = value.ToGuidOrNull();
        result = r ?? default;
        return r.HasValue;
    }

    #endregion

    /// <summary>
    /// Determines whether the object is equal to any of the provided values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">The object to be compared.</param>
    /// <param name="values">The values to compare with the object.</param>
    /// <returns></returns>
    public static bool In<T>(this T obj, params T[] values)
    {
        return Array.IndexOf(values, obj) != -1;
    }

    /// <summary>
    /// Determines whether the object is equal to none of the provided values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">The object to be compared.</param>
    /// <param name="values">The values to compare with the object.</param>
    /// <returns></returns>
    public static bool NotIn<T>(this T obj, params T[] values)
    {
        return obj.In(values) == false;
    }

    #region 泛型

    /// <summary>
    /// 【严格转换派】将输入对象严格转换为目标泛型类型。
    /// 失败时立刻抛出最精准的底层异常。
    /// </summary>
    public static T ToTarget<T>(this object? value)
    {
        // 1. 统一拦截 null 状态，针对非可空值类型提供强防御
        if (value is null || value is DBNull)
        {
            if (default(T) is null) return default!; // 目标是引用类型或 int? 等可空值类型，安全返回 null
            throw new ArgumentNullException(nameof(value), $"Cannot convert null to non-nullable type '{typeof(T).Name}'.");
        }

        // 2. 完美复用我们之前高度优化的强类型 TypeConverter 核心转换通道
        if (TypeConverter.TryConvertTo(value, typeof(T), out var result))
        {
            return (T)result!;
        }

        // 3. 彻底失败时，执行有损转换以引发最精准的原生转换异常，不悄悄吞掉系统级错误
        var actualType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T)Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 【可空宽容派】全网长尾类型的终极消化胃囊。
    /// 绝不抛出任何异常，转不了直接返回默认值（如果是值类型，返回的是 T 的 default）。
    /// </summary>
    public static T? ToTargetOrNull<T>(this object? value)
    {
        // 复用底层核心，彻底解决之前针对值类型返回 0 被误判为非 null 的致命逻辑缺陷
        return TypeConverter.TryConvertTo(value, typeof(T), out var result) ? (T?)result : default;
    }

    /// <summary>
    /// 【固定默认值派】单行漏斗：成功返回值，失败返回用户指定的固定默认值。
    /// </summary>
    public static T ToTargetOrDefault<T>(this object? value, T defaultValue = default!)
    {
        // 核心修正：不能使用 ?? 判断，必须使用布尔 Try 状态作为流控依据，确保数值 0 不会截断默认值逻辑
        return TypeConverter.TryConvertTo(value, typeof(T), out var result) ? (T)result! : defaultValue;
    }

    /// <summary>
    /// 【标准布尔控流派】单行漏斗：成功返回 true 且 out 输出正确值，失败返回 false 且 out 输出 default。
    /// </summary>
    public static bool TryToTarget<T>(this object? value, [NotNullWhen(true)] out T? result)
    {
        // 1. 核心路由：先让底层去尝试解包
        if (TypeConverter.TryConvertTo(value, typeof(T), out var rawResult))
        {
            // 2. 核心精细控流：如果底层解包出来是一个 null（对应输入 null 或 ""）
            if (rawResult is null)
            {
                // 检查当前的 T 到底能不能容纳 null 
                // 如果 default(T) 是 null（说明 T 是引用类型 string 或可空值类型 double?），这属于完全合法的空转换，返回 true
                if (default(T) is null)
                {
                    result = default;
                    return true;
                }

                // 💥 如果 default(T) 不是 null（说明 T 是普通的 double/int 等不可空值类型），
                // 此时外层明确拒绝放行，完美触发熔断，安全返回 false！
                result = default;
                return false;
            }

            result = (T?)rawResult;
            return true;
        }

        result = default;
        return false;
    }

    #endregion
}
