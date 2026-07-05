// Copyright (c) Linger. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// 提供统一的类型转换工具，用于在不同类型之间转换值。
/// </summary>
/// <remarks>
/// 此类集中处理类型转换逻辑，避免解决方案中的代码重复。
/// 支持可空类型、枚举、常见基元类型，并为其他类型提供后备转换。
/// </remarks>
/// <example>
/// <code>
/// // 将字符串转换为 int
/// object? result = TypeConverter.ConvertTo("123", typeof(int)); // 返回 123
/// 
/// // 转换可空类型
/// object? result = TypeConverter.ConvertTo(null, typeof(int?)); // 返回 null
/// 
/// // 转换为枚举
/// object? result = TypeConverter.ConvertTo("Monday", typeof(DayOfWeek)); // 返回 DayOfWeek.Monday
/// </code>
/// </example>
public static class TypeConverter
{
    // 将高频生成的 DateTime 工业模板固化到静态区，避免每次执行分支时在堆上创建数组
    private static readonly string[] s_extraDateTimeFormats = new[]
    {
        "M/d/yyyy h:mm:ss tt",
        "yyyy/M/d H:mm:ss",
        "yyyy/M/d h:mm:ss tt",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss"
    };

    /// <summary>
    /// 【严格转换派】将输入对象严格转换为目标类型。
    /// 失败时立刻抛出最精准的底层异常。
    /// </summary>
    /// <exception cref="InvalidCastException">当无法完成转换时抛出。</exception>
    public static object? ConvertTo(object? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (TryConvertTo(value, targetType, out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Cannot convert value '{value ?? "null"}' (Type: {value?.GetType().Name ?? "unknown"}) to target type '{targetType.Name}'.");
    }

    /// <summary>
    /// 【标准布尔控流派】尝试将值转换为指定的目标类型（零反射字符串分配，高性能版）。
    /// </summary>
    public static bool TryConvertTo(object? value, Type targetType, out object? result)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        // 1. 统一拦截 null 和 DBNull
        if (value is null || value is DBNull)
        {
            result = null;
            return true;
        }

        // 2. 统一拦截精确类型匹配（最快无损通道）
        var sourceType = value.GetType();
        if (sourceType == targetType)
        {
            result = value;
            return true;
        }

        // 3. 处理可空类型的核心底层拆箱（如 int? 则提取出 int）
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // 再次确认拆箱后的类型是否一致（应对 int? 传 int 的场景）
        if (sourceType == actualType)
        {
            result = value;
            return true;
        }

        try
        {
            // 4. 特殊处理：枚举类型（融入安全边界拦截）
            if (actualType.IsEnum)
            {
                if (TryConvertToEnum(value, actualType, out var enumResult))
                {
                    result = enumResult;
                    return true;
                }
                result = null;
                return false;
            }

            // 5. 特殊处理：Excel OADate 转换（双精度浮点转日期）
            if (actualType == typeof(DateTime) && value is double doubleValue)
            {
                result = DateTime.FromOADate(doubleValue);
                return true;
            }

            // 6. 执行核心类型严格转换
            return TryConvertToType(value, sourceType, actualType, out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// 核心私有路由：执行针对基础强类型的零无端分配安全分流
    /// </summary>
    private static bool TryConvertToType(object value, Type sourceType, Type actualType, out object? result)
    {
        // 提取并缓存字符串状态，杜绝后续多分支高频触发无端的 ToString() 内存爆炸
        string? stringValueCache = value as string;

        // 字符串转换 - 始终成功
        if (actualType == typeof(string))
        {
            result = stringValueCache ?? value.ToString();
            return true;
        }

        // 黄金性能通道：针对已知的底层基础强类型互转，优先使用 C# 原生模式匹配
        // 这种基于运行时类型直接拆箱（Unboxing）的做法效率最高，属于编译期级别的优化
        switch (value)
        {
            case int i when actualType == typeof(int): result = i; return true;
            case long l when actualType == typeof(long): result = l; return true;
            case decimal d when actualType == typeof(decimal): result = d; return true;
            case double d when actualType == typeof(double): result = d; return true;
            case float f when actualType == typeof(float): result = f; return true;
            case bool b when actualType == typeof(bool): result = b; return true;
            case DateTime dt when actualType == typeof(DateTime): result = dt; return true;
            case Guid g when actualType == typeof(Guid): result = g; return true;
        }

        // 如果上面匹配失败，说明数据源变量确实是以非目标形式存在（如 value 是一串文本 "123"），我们统一安全初始化文本视图
        stringValueCache ??= value.ToString() ?? string.Empty;

        // ================== 开始精确、高效的内置强类型反向解析 ==================

        if (actualType == typeof(int))
        {
            if (int.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(long))
        {
            if (long.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(decimal))
        {
            if (decimal.TryParse(stringValueCache, NumberStyles.Number, CultureInfo.InvariantCulture, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(double))
        {
            if (value is float f) { result = (double)f; return true; } // 隐式级别向上提升
            if (double.TryParse(stringValueCache, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(float))
        {
            if (float.TryParse(stringValueCache, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(bool))
        {
            var str = stringValueCache.Trim().ToLowerInvariant();
            result = str switch
            {
                "true" or "1" or "yes" or "y" => true,
                "false" or "0" or "no" or "n" => false,
                _ => null
            };
            return result is not null;
        }

        if (actualType == typeof(DateTime))
        {
            string strValue = stringValueCache.Trim();
            if (DateTime.TryParse(strValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateResult)) { result = dateResult; return true; }
            if (DateTime.TryParse(strValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateResult)) { result = dateResult; return true; }
            if (DateTime.TryParseExact(strValue, s_extraDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateResult)) { result = dateResult; return true; }
            result = null; return false;
        }

        if (actualType == typeof(Guid))
        {
            if (Guid.TryParse(stringValueCache, out var r)) { result = r; return true; }
            result = null; return false;
        }

        if (actualType == typeof(short)) { if (short.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }
        if (actualType == typeof(byte)) { if (byte.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }
        if (actualType == typeof(sbyte)) { if (sbyte.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }
        if (actualType == typeof(ushort)) { if (ushort.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }
        if (actualType == typeof(uint)) { if (uint.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }
        if (actualType == typeof(ulong)) { if (ulong.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) { result = r; return true; } result = null; return false; }

        // ================== 长尾长效吸收区：融入泛型版本的经典兼容层 ==================

        // 兼容场景 A：处理特殊的 TimeSpan 类型
        if (actualType == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(stringValueCache.Trim(), CultureInfo.InvariantCulture, out var tsResult)) { result = tsResult; return true; }
            result = null; return false;
        }

        // 兼容场景 B：高度包容性的 TypeConverter 机制（完美支撑挂载了转换器特性的复杂自定义对象）
        var converter = TypeDescriptor.GetConverter(actualType);
        if (value is string && converter.CanConvertFrom(typeof(string)))
        {
            var converted = converter.ConvertFromInvariantString(stringValueCache);
            if (converted != null) { result = converted; return true; }
        }
        if (converter.CanConvertFrom(sourceType))
        {
            var converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            if (converted != null) { result = converted; return true; }
        }

        // 7. 终极兜底策略：如果是其他冷门基础结构体，走原生的 ChangeType
        result = Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
        return true;
    }

    /// <summary>
    /// 处理枚举转换，封堵了原生的任意垃圾数字随意强转的安全隐患
    /// </summary>
    private static bool TryConvertToEnum(object value, Type enumType, out object? result)
    {
        try
        {
            if (value is string stringValue)
            {
#if NET8_0_OR_GREATER
                return Enum.TryParse(enumType, stringValue, ignoreCase: true, out result);
#else
                result = Enum.Parse(enumType, stringValue, ignoreCase: true);
                return true;
#endif
            }

            // 针对数字传入枚举，增加 IsDefined 严密把关，防止类似 999 被注入为合法枚举
            if (Enum.IsDefined(enumType, value))
            {
                result = Enum.ToObject(enumType, value);
                return true;
            }

            result = null;
            return false;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// 将值转换为指定类型，处理可空类型。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="conversionType">要转换到的类型。</param>
    /// <returns>转换后的值。</returns>
    /// <remarks>
    /// 此方法为向后兼容而保留，适用于使用 <see cref="System.ComponentModel.NullableConverter"/> 的现有代码。
    /// 对于新代码，请优先使用 <see cref="ConvertTo"/>。
    /// </remarks>
    [Obsolete("请使用 ConvertTo 代替。此方法为向后兼容而保留。")]
    public static object? ConvertToType(object? value, Type conversionType)
    {
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (value.IsNull())
            {
                return null;
            }

            var nullableConverter = new NullableConverter(conversionType);
            conversionType = nullableConverter.UnderlyingType;
        }

        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }
}
