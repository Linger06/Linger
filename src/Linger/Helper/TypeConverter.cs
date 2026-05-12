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
    /// <summary>
    /// 将值转换为指定的目标类型。
    /// </summary>
    /// <param name="value">要转换的值。可以是 null 或 DBNull。</param>
    /// <param name="targetType">要转换到的目标类型。</param>
    /// <returns>
    /// 转换后的值；如果输入是 null/DBNull 且目标类型可空，则返回 null。
    /// </returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="targetType"/> 为 null 时抛出。</exception>
    /// <exception cref="InvalidCastException">当转换失败且没有后备方案时抛出。</exception>
    /// <example>
    /// <code>
    /// // 基本转换
    /// var intValue = TypeConverter.ConvertTo("42", typeof(int));       // 返回 42
    /// var boolValue = TypeConverter.ConvertTo("true", typeof(bool));   // 返回 true
    /// var dateValue = TypeConverter.ConvertTo("2024-01-01", typeof(DateTime));
    /// 
    /// // 可空类型处理
    /// var nullableInt = TypeConverter.ConvertTo(null, typeof(int?));   // 返回 null
    /// var nullableDate = TypeConverter.ConvertTo(DBNull.Value, typeof(DateTime?)); // 返回 null
    /// 
    /// // 枚举转换
    /// var dayOfWeek = TypeConverter.ConvertTo("Friday", typeof(DayOfWeek)); // 返回 DayOfWeek.Friday
    /// var dayFromInt = TypeConverter.ConvertTo(5, typeof(DayOfWeek));       // 返回 DayOfWeek.Friday
    /// </code>
    /// </example>
    public static object? ConvertTo(object? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        // Handle null and DBNull
        if (value is null || value is DBNull)
        {
            return null;
        }

        // Fast path for exact type matches (avoids conversion overhead)
        if (value.GetType() == targetType)
        {
            return value;
        }

        // Handle nullable types - get the underlying type
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle enums specifically
        if (actualType.IsEnum)
        {
            if (TryConvertToEnum(value, actualType, out var enumResult))
            {
                return enumResult;
            }

            throw new InvalidCastException($"Cannot convert '{value}' to enum type '{actualType.Name}'.");
        }

        // Special case: Converting double to DateTime (Excel OADate format)
        if (actualType == typeof(DateTime) && value is double doubleValue)
        {
            return DateTime.FromOADate(doubleValue);
        }

        // Optimized conversion using pattern matching for common types
        return actualType.Name switch
        {
            nameof(String) => value.ToStringOrDefault(),
            nameof(Int16) => value.ToShortOrDefault(),
            nameof(Int32) => value.ToIntOrDefault(),
            nameof(Int64) => value.ToLongOrDefault(),
            nameof(Single) => value.ToFloatOrDefault(),
            nameof(Double) => value.ToDoubleOrDefault(),
            nameof(Decimal) => value.ToDecimalOrDefault(),
            nameof(Boolean) => value.ToBoolOrDefault(),
            nameof(DateTime) => value.ToDateTimeOrDefault(),
            nameof(Guid) => value.ToGuidOrDefault(),
            nameof(Byte) => value.ToByteOrDefault(),
            nameof(SByte) => value.ToSByteOrDefault(),
            nameof(UInt16) => value.ToUShortOrDefault(),
            nameof(UInt32) => value.ToUIntOrDefault(),
            nameof(UInt64) => value.ToULongOrDefault(),
            _ => Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture) // Fallback for other types (including Char)
        };
    }

    /// <summary>
    /// 尝试将值转换为指定的目标类型。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">要转换到的目标类型。</param>
    /// <param name="result">当此方法返回时，如果转换成功则包含转换后的值；否则为 null。</param>
    /// <returns>如果转换成功返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    /// <remarks>
    /// 此方法遵循标准的 Try 模式：返回布尔值表示成功与否，
    /// 并通过 out 参数输出结果。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 标准 Try 模式用法
    /// if (TypeConverter.TryConvertTo("123", typeof(int), out var result))
    /// {
    ///     Console.WriteLine($"转换后的值: {result}"); // 123
    /// }
    /// else
    /// {
    ///     Console.WriteLine("转换失败");
    /// }
    /// 
    /// // 无效转换返回 false
    /// bool success = TypeConverter.TryConvertTo("abc", typeof(int), out var value); // false
    /// </code>
    /// </example>
    public static bool TryConvertTo(object? value, Type targetType, out object? result)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        // Handle null and DBNull - this is a valid "conversion" to null
        if (value is null || value is DBNull)
        {
            result = null;
            return true;
        }

        // Fast path for exact type matches
        if (value.GetType() == targetType)
        {
            result = value;
            return true;
        }

        // Handle nullable types - get the underlying type
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            // Handle enums specifically
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

            // Special case: Converting double to DateTime (Excel OADate format)
            if (actualType == typeof(DateTime) && value is double doubleValue)
            {
                result = DateTime.FromOADate(doubleValue);
                return true;
            }

            // Use strict conversion with TryTo methods
            return TryConvertToType(value, actualType, out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// 执行严格的类型转换，返回成功状态。
    /// </summary>
    private static bool TryConvertToType(object value, Type actualType, out object? result)
    {
        // 字符串转换 - 始终成功
        if (actualType == typeof(string))
        {
            result = value.ToString();
            return true;
        }

        // 使用 TryTo 方法进行严格验证
        if (actualType == typeof(int))
        {
            if (value.TryToInt(out var intResult))
            {
                result = intResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(long))
        {
            if (value.TryToLong(out var longResult))
            {
                result = longResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(decimal))
        {
            if (value.TryToDecimal(out var decimalResult))
            {
                result = decimalResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(double))
        {
            if (value is double d)
            {
                result = d;
                return true;
            }

            if (value is float f)
            {
                result = (double)f;
                return true;
            }

            if (double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
            {
                result = doubleResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(float))
        {
            if (value is float f)
            {
                result = f;
                return true;
            }

            if (float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
            {
                result = floatResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(bool))
        {
            if (value is bool b)
            {
                result = b;
                return true;
            }

            var str = value.ToString()?.Trim().ToLowerInvariant();
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
            if (value is DateTime dt)
            {
                result = dt;
                return true;
            }
            string strValue = value.ToString()?.Trim() ?? string.Empty;
            // 策略 1：使用固定区域性解析（兼容 en-US、标准 ISO、以及无歧义格式）
            if (DateTime.TryParse(strValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateResult))
            {
                result = dateResult;
                return true;
            }
            // 策略 2：【针对 zh-CN 生产环境修复】尝试用当前线程 Culture 解析（兼容带有"上午/下午"、"年/月/日"等本地格式）
            if (DateTime.TryParse(strValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateResult))
            {
                result = dateResult;
                return true;
            }
            // 策略 3：常见工业格式多模版精确匹配兜底
            string[] extraFormats = new[]
            {
                "M/d/yyyy h:mm:ss tt",
                "yyyy/M/d H:mm:ss",
                "yyyy/M/d h:mm:ss tt",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy/MM/dd HH:mm:ss"
            };
            if (DateTime.TryParseExact(strValue, extraFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateResult))
            {
                result = dateResult;
                return true;
            }
            // 彻底失败：不要改变 result，直接返回 false，交给外层决定是阻断还是记录
            result = null;
            return false;
        }

        if (actualType == typeof(Guid))
        {
            if (value is Guid g)
            {
                result = g;
                return true;
            }

            if (Guid.TryParse(value.ToString(), out var guidResult))
            {
                result = guidResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(short))
        {
            if (short.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var shortResult))
            {
                result = shortResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(byte))
        {
            if (byte.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteResult))
            {
                result = byteResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(sbyte))
        {
            if (sbyte.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sbyteResult))
            {
                result = sbyteResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(ushort))
        {
            if (ushort.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ushortResult))
            {
                result = ushortResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(uint))
        {
            if (uint.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var uintResult))
            {
                result = uintResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(ulong))
        {
            if (ulong.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulongResult))
            {
                result = ulongResult;
                return true;
            }

            result = null;
            return false;
        }

        // 后备方案：尝试 Convert.ChangeType
        try
        {
            result = Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// 尝试将值转换为指定的枚举类型。
    /// </summary>
    /// <param name="value">要转换的值（字符串或数字）。</param>
    /// <param name="enumType">要转换到的枚举类型。</param>
    /// <param name="result">当此方法返回时，如果成功则包含枚举值；否则为 null。</param>
    /// <returns>如果转换成功返回 <c>true</c>；否则返回 <c>false</c>。</returns>
    private static bool TryConvertToEnum(object value, Type enumType, out object? result)
    {
        try
        {
            if (value is string stringValue)
            {
#if NET8_0_OR_GREATER
                if (Enum.TryParse(enumType, stringValue, ignoreCase: true, out var enumResult))
                {
                    result = enumResult;
                    return true;
                }

                result = null;
                return false;
#else
                result = Enum.Parse(enumType, stringValue, ignoreCase: true);
                return true;
#endif
            }

            // 对于数字值，转换为枚举
            result = Enum.ToObject(enumType, value);
            return true;
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
