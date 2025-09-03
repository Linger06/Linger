using System.Collections.Concurrent;
using System.Reflection;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="object"/> extensions
/// </summary>
public static class ObjectExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_propertyCache = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> s_propertyMapCache = new();

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is not null.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is not null; otherwise, false.</returns>
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null; otherwise, false.</returns>
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is not null and its string representation is not empty.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is not null and its string representation is not empty; otherwise, false.</returns>
    [Obsolete("Use IsNotNullOrEmpty instead. Will be removed in 1.0.0.")]
    public static bool IsNotNullAndEmpty([NotNullWhen(true)] this object? value) => value.IsNotNullOrEmpty();

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is not null and its string representation is not empty.
    /// </summary>
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this object? value) => value is not null && !string.IsNullOrEmpty(value.ToString());

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null or its string representation is empty.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null or its string representation is empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value)
    {
        return value is null || string.IsNullOrEmpty(value.ToString());
    }

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null or <see cref="DBNull"/>.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null or DBNull; otherwise, false.</returns>
    public static bool IsNullOrDbNull([NotNullWhen(false)] this object? value)
    {
        return value is DBNull or null;
    }

    /// <summary>
    /// Executes a specified action on each property of the current object.
    /// </summary>
    /// <typeparam name="T">The type of the object to perform the action on.</typeparam>
    /// <param name="value">The object to perform the action on.</param>
    /// <param name="action">The <see cref="Action{T1, T2}"/> delegate to perform on each property of the current object.</param>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// obj.ForIn((name, val) => Console.WriteLine($"{name}: {val}"));
    /// // Output:
    /// // Name: John
    /// // Age: 30
    /// </code>
    /// </example>
    public static void ForIn<T>(this T? value, Action<string, object?> action)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(action);
        if (value is null)
        {
            return;
        }
        // Use the runtime type instead of the generic parameter type to ensure
        // that derived or anonymous types referenced through a base/interface
        // still expose their concrete public instance properties. This avoids
        // missing properties when the generic method is invoked via a base reference.
        // Cached by Type to retain performance characteristics.
        var runtimeType = value.GetType();
        var properties = s_propertyCache.GetOrAdd(runtimeType, static type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead)
            {
                var val = property.GetValue(value, null);
                action(property.Name, val);
            }
        }
    }
    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> of a specified property name with caching for performance.
    /// </summary>
    /// <param name="obj">The object to get the property info from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The <see cref="PropertyInfo"/> of the specified property.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property name does not exist.</exception>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John" };
    /// var propertyInfo = obj.GetPropertyInfo("Name");
    /// Console.WriteLine(propertyInfo.Name); // Output: Name
    /// </code>
    /// </example>
    public static PropertyInfo GetPropertyInfo(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);
        var type = obj.GetType();
        var map = s_propertyMapCache.GetOrAdd(type, static t =>
        {
            var props = s_propertyCache.GetOrAdd(t, static inner => inner.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            var dict = new Dictionary<string, PropertyInfo>(props.Length, StringComparer.Ordinal);
            foreach (var p in props)
            {
                dict[p.Name] = p;
            }
            return dict;
        });
        if (map.TryGetValue(propertyName, out var pi))
        {
            return pi;
        }
        throw new InvalidOperationException($"Property '{propertyName}' does not exist on type '{type.Name}'");
    }

    /// <summary>
    /// Gets the value of a specified property.
    /// </summary>
    /// <param name="obj">The object to get the property value from.</param>
    /// /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the specified property.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John" };
    /// var value = obj.GetPropertyValue("Name");
    /// Console.WriteLine(value); // Output: John
    /// </code>
    /// </example>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        return obj.GetPropertyInfo(propertyName).GetValue(obj, null);
    }

    #region Type checking methods
    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="string"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="string"/> type; otherwise, false.</returns>
    public static bool IsString(this object? value) => value is string;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="short"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="short"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsInt16(this object? value) => value is short;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="int"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="int"/> type; otherwise, false.</returns>
    public static bool IsInt(this object? value) => value is int;

    /// <summary>
    /// 判断对象是否为任一有符号整数 (short / int / long)。
    /// </summary>
    public static bool IsAnySignedInteger(this object? value) => value is short or int or long;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="long"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="long"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsInt64(this object? value) => value is long;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="decimal"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="decimal"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsDecimal(this object? value) => value is decimal;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="float"/> type; otherwise, false.</returns>
    [Obsolete("Use IsFloat() instead. Will be removed in 1.0.0.")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsSingle(this object? value) => IsFloat(value);

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="float"/> type; otherwise, false.</returns>
    public static bool IsFloat(this object? value) => value is float;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="double"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="double"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsDouble(this object? value) => value is double;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="DateTime"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="DateTime"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsDateTime(this object? value) => value is DateTime;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="bool"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="bool"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsBoolean(this object? value) => value is bool;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="Guid"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="Guid"/> type; otherwise, false.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static bool IsGuid(this object? value) => value is Guid;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is a numeric type (byte, short, int, long, float, double, decimal).
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is a numeric type; otherwise, false.</returns>
    public static bool IsNumeric(this object? value) => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    /// <summary>
    /// 判断对象是否为任一无符号整数 (byte / ushort / uint / ulong)。
    /// </summary>
    public static bool IsAnyUnsignedInteger(this object? value) => value is byte or ushort or uint or ulong;

    /// <summary>
    /// 判断对象是否为 <see cref="byte"/> 类型。
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is a byte; otherwise, false.</returns>
    public static bool IsByte(this object? value) => value is byte;

    /// <summary>
    /// 判断对象是否为 <see cref="sbyte"/> 类型。
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is a signed byte; otherwise, false.</returns>
    public static bool IsSByte(this object? value) => value is sbyte;

    /// <summary>
    /// 判断对象是否为 <see cref="ushort"/> 类型。
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is an unsigned short; otherwise, false.</returns>
    public static bool IsUShort(this object? value) => value is ushort;

    /// <summary>
    /// 判断对象是否为 <see cref="uint"/> 类型。
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is an unsigned int; otherwise, false.</returns>
    public static bool IsUInt(this object? value) => value is uint;

    /// <summary>
    /// 判断对象是否为 <see cref="ulong"/> 类型。
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is an unsigned long; otherwise, false.</returns>
    public static bool IsULong(this object? value) => value is ulong;

    #endregion
    /// <summary>
    /// Converts the input object to a trimmed string. Returns an empty string if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A trimmed string representation of the input object, or an empty string if the input is null.</returns>
    /// <example>
    /// <code>
    /// object obj = "  Hello World  ";
    /// string result = obj.ToTrimmedString();
    /// Console.WriteLine(result); // Output: "Hello World"
    /// </code>
    /// </example>
    public static string ToTrimmedString(this object? input) => input?.ToString()?.Trim() ?? string.Empty;

    /// <summary>
    /// Converts the input object to a trimmed string. Returns an empty string if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A trimmed string representation of the input object, or an empty string if the input is null.</returns>
    /// <example>
    /// <code>
    /// object obj = "  Hello World  ";
    /// string result = obj.ToNotSpaceString();
    /// Console.WriteLine(result); // Output: "Hello World"
    /// </code>
    /// </example>
    [Obsolete("Use ToTrimmedString() instead. Will be removed in 1.0.0.")]
    public static string ToNotSpaceString(this object? input) => input?.ToString()?.Trim() ?? string.Empty;

    /// <summary>
    /// Converts the input object to a string. Returns the specified default value if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the input is null.</param>
    /// <returns>A string representation of the input object, or the specified default value if the input is null.</returns>
    /// <example>
    /// <code>
    /// object? obj = null;
    /// string result = obj.ToStringOrDefault("Default");
    /// Console.WriteLine(result); // Output: "Default"
    /// </code>
    /// </example>
    public static string ToStringOrDefault(this object? input, string defaultValue = "") => input?.ToString() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a string. Returns the specified default value if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the input is null.</param>
    /// <returns>A string representation of the input object, or the specified default value if the input is null.</returns>
    [Obsolete("Use ToStringOrDefault instead. Will be removed in 1.0.0.")]
    public static string ToSafeString(this object? input, string defaultValue = "") => ToStringOrDefault(input, defaultValue);
    /// <summary>
    /// Converts the input object to a string. Returns an empty string if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A string representation of the input object, or an empty string if the input is null.</returns>
    /// <example>
    /// <code>
    /// object? obj = 42;
    /// string result = obj.ToStringOrEmpty();
    /// Console.WriteLine(result); // Output: "42"
    /// </code>
    /// </example>
    [Obsolete("Use ToStringOrDefault() instead. Will be removed in 1.0.0.")]
    public static string ToStringOrEmpty(this object? input) => input?.ToString() ?? string.Empty;
    /// <summary>
    /// Converts the input object to a string. Returns null if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A string representation of the input object, or null if the input is null.</returns>
    /// <example>
    /// <code>
    /// object? obj = null;
    /// string? result = obj.ToStringOrNull();
    /// Console.WriteLine(result ?? "Was null"); // Output: "Was null"
    /// </code>
    /// </example>
    public static string? ToStringOrNull(this object? input) => input?.ToString();

    /// <summary>
    /// Converts the input object to a normalized string with optional trimming and null handling.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="trim">Whether to trim whitespace from the result.</param>
    /// <param name="treatEmptyAsNull">Whether to treat empty strings as null.</param>
    /// <returns>A normalized string representation of the input object.</returns>
    /// <example>
    /// <code>
    /// object obj = "  Hello World  ";
    /// string? result = obj.ToNormalizedString(trim: true, treatEmptyAsNull: true);
    /// Console.WriteLine(result); // Output: "Hello World"
    /// </code>
    /// </example>
    public static string? ToNormalizedString(this object? input, bool trim = false, bool treatEmptyAsNull = false)
    {
        var result = input?.ToString();

        if (trim)
        {
            result = result?.Trim();
        }

        if (treatEmptyAsNull && string.IsNullOrEmpty(result))
        {
            return null;
        }

        return result;
    }

    /// <summary>
    /// Converts the input object to a short. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A short representation of the input object, or the specified default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// short result = obj.ToShortOrDefault();
    /// Console.WriteLine(result); // Output: 123
    /// </code>
    /// </example>
    public static short ToShortOrDefault(this object? input, short defaultValue = 0) => input.ToShortOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a short. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A short representation of the input object, or the specified default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// short result = obj.ToShort();
    /// Console.WriteLine(result); // Output: 123
    /// </code>
    /// </example>
    [Obsolete("Use ToShortOrDefault instead. Will be removed in 1.0.0.")]
    public static short ToShort(this object? input, short defaultValue = 0) => input.ToShortOrDefault(defaultValue);

    /// <summary>
    /// Converts the input object to a nullable short. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable short representation of the input object, or null if the conversion fails.</returns>
    public static short? ToShortOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is short shortValue)
        {
            return shortValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToShortOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="short"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted short value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToShort(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToShort(this object? input, out short value)
    {
        var r = input.ToShortOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Converts the input object to a long. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A long representation of the input object, or the specified default value if the conversion fails.</returns>
    public static long ToLongOrDefault(this object? input, long defaultValue = 0) => input.ToLongOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a long. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A long representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToLongOrDefault instead. Will be removed in 1.0.0.")]
    public static long ToLong(this object? input, long defaultValue = 0) => ToLongOrDefault(input, defaultValue);

    /// <summary>
    /// Converts the input object to a nullable long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable long representation of the input object, or null if the conversion fails.</returns>
    public static long? ToLongOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is long longValue)
        {
            return longValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToLongOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="long"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted long value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToLong(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToLong(this object? input, out long value)
    {
        var r = input.ToLongOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    public static decimal ToDecimalOrDefault(this object? input, int? digits = null)
    {
        return ToDecimalOrDefault(input, 0, digits);
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToDecimalOrDefault instead. Will be removed in 1.0.0.")]
    public static decimal ToDecimal(this object? input, int? digits = null)
    {
        return ToDecimalOrDefault(input, digits);
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    public static decimal ToDecimalOrDefault(this object? input, decimal defaultValue, int? digits = null)
    {
        return input.ToStringOrNull().ToDecimalOrDefault(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToDecimalOrDefault instead. Will be removed in 1.0.0.")]
    public static decimal ToDecimal(this object? input, decimal defaultValue, int? digits = null)
    {
        return ToDecimalOrDefault(input, defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable decimal. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable decimal representation of the input object, or null if the conversion fails.</returns>
    public static decimal? ToDecimalOrNull(this object? input, decimal? defaultValue = null, int? digits = null)
    {
        // Performance optimization: check direct type match first
        if (input is decimal decimalValue)
        {
            return digits.HasValue ? Math.Round(decimalValue, digits.Value) : decimalValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToDecimalOrNull(defaultValue, digits);
    }

    /// <summary>
    /// 尝试转换为 <see cref="decimal"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted decimal value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "123.45";
    /// if (obj.TryToDecimal(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToDecimal(this object? input, out decimal value)
    {
        var r = input.ToDecimalOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0m;
        return false;
    }

    /// <summary>
    /// Converts the input object to an integer. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>An integer representation of the input object, or the specified default value if the conversion fails.</returns>
    public static int ToIntOrDefault(this object? input, int defaultValue = 0) => input.ToIntOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to an integer. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>An integer representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToIntOrDefault instead. Will be removed in 1.0.0.")]
    public static int ToInt(this object? input, int defaultValue = 0) => ToIntOrDefault(input, defaultValue);

    /// <summary>
    /// Converts the input object to a nullable integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable integer representation of the input object, or null if the conversion fails.</returns>
    public static int? ToIntOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is int intValue)
        {
            return intValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToIntOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="int"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted int value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "42";
    /// if (obj.TryToInt(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToInt(this object? input, out int value)
    {
        var r = input.ToIntOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Converts the input object to a double. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A double representation of the input object, or the specified default value if the conversion fails.</returns>
    public static double ToDoubleOrDefault(this object? input, double defaultValue = 0, int? digits = null)
    {
        return input.ToStringOrNull().ToDoubleOrDefault(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a double. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A double representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToDoubleOrDefault instead. Will be removed in 1.0.0.")]
    public static double ToDouble(this object? input, double defaultValue = 0, int? digits = null)
    {
        return ToDoubleOrDefault(input, defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable double. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable double representation of the input object, or null if the conversion fails.</returns>
    public static double? ToDoubleOrNull(this object? input, double? defaultValue = null, int? digits = null)
    {
        // Performance optimization: check direct type match first
        if (input is double doubleValue)
        {
            return digits.HasValue ? Math.Round(doubleValue, digits.Value) : doubleValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToDoubleOrNull(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    public static float ToFloatOrDefault(this object? input, int? digits = null)
    {
        return ToFloatOrDefault(input, 0, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToFloatOrDefault instead. Will be removed in 1.0.0.")]
    public static float ToFloat(this object? input, int? digits = null)
    {
        return ToFloatOrDefault(input, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    public static float ToFloatOrDefault(this object? input, float defaultValue, int? digits = null)
    {
        return input.ToStringOrNull().ToFloatOrDefault(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToFloatOrDefault instead. Will be removed in 1.0.0.")]
    public static float ToFloat(this object? input, float defaultValue, int? digits = null)
    {
        return ToFloatOrDefault(input, defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable float. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable float representation of the input object, or null if the conversion fails.</returns>
    public static float? ToFloatOrNull(this object? input, float? defaultValue = null, int? digits = null)
    {
        // Performance optimization: check direct type match first
        if (input is float floatValue)
        {
            return digits.HasValue ? (float)Math.Round(floatValue, digits.Value) : floatValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToFloatOrNull(defaultValue, digits);
    }    /// <summary>
         /// Converts the input object to a DateTime. Returns the specified default value if the conversion fails.
         /// </summary>
         /// <param name="input">The input object.</param>
         /// <param name="defaultValue">The default value to return if the conversion fails. Defaults to DateTime.MinValue.</param>
         /// <returns>A DateTime representation of the input object, or the specified default value if the conversion fails.</returns>
    public static DateTime ToDateTimeOrDefault(this object? input, DateTime? defaultValue = null)
    {
        return ToDateTimeOrNull(input) ?? defaultValue ?? DateTime.MinValue;
    }

    /// <summary>
    /// Converts the input object to a DateTime. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails. Defaults to DateTime.MinValue.</param>
    /// <returns>A DateTime representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToDateTimeOrDefault instead. Will be removed in 1.0.0.")]
    public static DateTime ToDateTime(this object? input, DateTime? defaultValue = null)
    {
        return ToDateTimeOrDefault(input, defaultValue);
    }

    /// <summary>
    /// Converts the input object to a nullable DateTime. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable DateTime representation of the input object, or null if the conversion fails.</returns>
    public static DateTime? ToDateTimeOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is DateTime dateTimeValue)
        {
            return dateTimeValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToDateTimeOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="DateTime"/>。成功返回 true 并输出值；失败返回 false，输出 DateTime.MinValue。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted DateTime value if successful, or DateTime.MinValue if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "2023-12-25";
    /// if (obj.TryToDateTime(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToDateTime(this object? input, out DateTime value)
    {
        var r = input.ToDateTimeOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = DateTime.MinValue;
        return false;
    }

    /// <summary>
    /// Converts the input object to a boolean. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A boolean representation of the input object, or the specified default value if the conversion fails.</returns>
    public static bool ToBoolOrDefault(this object? input, bool defaultValue = false) => input.ToBoolOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a boolean. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A boolean representation of the input object, or the specified default value if the conversion fails.</returns>
    [Obsolete("Use ToBoolOrDefault instead. Will be removed in 1.0.0.")]
    public static bool ToBool(this object? input, bool defaultValue = false) => ToBoolOrDefault(input, defaultValue);

    /// <summary>
    /// Converts the input object to a nullable boolean. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable boolean representation of the input object, or null if the conversion fails.</returns>
    public static bool? ToBoolOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is bool boolValue)
        {
            return boolValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToBoolOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="bool"/>。成功返回 true 并输出值；失败返回 false，输出 false。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted bool value if successful, or false if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "true";
    /// if (obj.TryToBool(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToBool(this object? input, out bool value)
    {
        var r = input.ToBoolOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = false;
        return false;
    }

    /// <summary>
    /// Converts the input object to a Guid. Returns Guid.Empty if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A Guid representation of the input object, or Guid.Empty if the conversion fails.</returns>
    public static Guid ToGuidOrDefault(this object? input)
    {
        return ToGuidOrNull(input) ?? Guid.Empty;
    }

    /// <summary>
    /// Converts the input object to a Guid. Returns Guid.Empty if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A Guid representation of the input object, or Guid.Empty if the conversion fails.</returns>
    [Obsolete("Use ToGuidOrDefault instead. Will be removed in 1.0.0.")]
    public static Guid ToGuid(this object? input)
    {
        return ToGuidOrDefault(input);
    }

    /// <summary>
    /// Converts the input object to a nullable Guid. Returns null if the conversion fails.
    /// </summary>
    /// <param name="value">The input object.</param>
    /// <returns>A nullable Guid representation of the input object, or null if the conversion fails.</returns>
    public static Guid? ToGuidOrNull(this object? value)
    {
        // Performance optimization: check direct type match first
        if (value is Guid guidValue)
        {
            return guidValue;
        }

        // Fall back to string conversion for other types
        return value.ToStringOrNull().ToGuidOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="Guid"/>。成功返回 true 并输出值；失败返回 false，输出 Guid.Empty。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted Guid value if successful, or Guid.Empty if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "550e8400-e29b-41d4-a716-446655440000";
    /// if (obj.TryToGuid(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToGuid(this object? input, out Guid value)
    {
        var r = input.ToGuidOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = Guid.Empty;
        return false;
    }

    /// <summary>
    /// 尝试转换为 <see cref="double"/>。成功返回 true 并输出值；失败返回 false，输出 0.0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted double value if successful, or 0.0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "123.456";
    /// if (obj.TryToDouble(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToDouble(this object? input, out double value)
    {
        var r = input.ToDoubleOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0.0;
        return false;
    }

    /// <summary>
    /// 尝试转换为 <see cref="float"/>。成功返回 true 并输出值；失败返回 false，输出 0.0f。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted float value if successful, or 0.0f if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object obj = "12.34";
    /// if (obj.TryToFloat(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Conversion failed");
    /// }
    /// </code>
    /// </example>
    public static bool TryToFloat(this object? input, out float value)
    {
        var r = input.ToFloatOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0.0f;
        return false;
    }

    #region Byte Extensions

    /// <summary>
    /// Converts the input object to byte with a default value.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A byte representation of the input object, or the default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// byte result = obj.ToByteOrDefault(0); // result is 123
    ///
    /// object invalid = "abc";
    /// byte result2 = invalid.ToByteOrDefault(0); // result2 is 0
    /// </code>
    /// </example>
    public static byte ToByteOrDefault(this object? input, byte defaultValue = 0) => input.ToByteOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a nullable byte. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable byte representation of the input object, or null if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// byte? result = obj.ToByteOrNull(); // result is 123
    ///
    /// object invalid = "abc";
    /// byte? result2 = invalid.ToByteOrNull(); // result2 is null
    /// </code>
    /// </example>
    public static byte? ToByteOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is byte byteValue)
        {
            return byteValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToByteOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="byte"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted byte value if successful, or 0 if failed.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToByte(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}"); // Output: Converted: 123
    /// }
    /// </code>
    /// </example>
    public static bool TryToByte(this object? input, out byte value)
    {
        var r = input.ToByteOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region SByte Extensions

    /// <summary>
    /// Converts the input object to sbyte with a default value.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A sbyte representation of the input object, or the default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// sbyte result = obj.ToSByteOrDefault(0); // result is 123
    ///
    /// object invalid = "abc";
    /// sbyte result2 = invalid.ToSByteOrDefault(0); // result2 is 0
    /// </code>
    /// </example>
    public static sbyte ToSByteOrDefault(this object? input, sbyte defaultValue = 0) => input.ToSByteOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a nullable sbyte. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable sbyte representation of the input object, or null if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// sbyte? result = obj.ToSByteOrNull(); // result is 123
    ///
    /// object invalid = "abc";
    /// sbyte? result2 = invalid.ToSByteOrNull(); // result2 is null
    /// </code>
    /// </example>
    public static sbyte? ToSByteOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is sbyte sbyteValue)
        {
            return sbyteValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToSByteOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="sbyte"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted sbyte value if successful, or 0 if failed.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToSByte(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}"); // Output: Converted: 123
    /// }
    /// </code>
    /// </example>
    public static bool TryToSByte(this object? input, out sbyte value)
    {
        var r = input.ToSByteOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region UInt Extensions

    /// <summary>
    /// Converts the input object to uint with a default value.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A uint representation of the input object, or the default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// uint result = obj.ToUIntOrDefault(0); // result is 123
    ///
    /// object invalid = "abc";
    /// uint result2 = invalid.ToUIntOrDefault(0); // result2 is 0
    /// </code>
    /// </example>
    public static uint ToUIntOrDefault(this object? input, uint defaultValue = 0) => input.ToUIntOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a nullable uint. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable uint representation of the input object, or null if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// uint? result = obj.ToUIntOrNull(); // result is 123
    ///
    /// object invalid = "abc";
    /// uint? result2 = invalid.ToUIntOrNull(); // result2 is null
    /// </code>
    /// </example>
    public static uint? ToUIntOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is uint uintValue)
        {
            return uintValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToUIntOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="uint"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted uint value if successful, or 0 if failed.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToUInt(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}"); // Output: Converted: 123
    /// }
    /// </code>
    /// </example>
    public static bool TryToUInt(this object? input, out uint value)
    {
        var r = input.ToUIntOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region ULong Extensions

    /// <summary>
    /// Converts the input object to ulong with a default value.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A ulong representation of the input object, or the default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// ulong result = obj.ToULongOrDefault(0); // result is 123
    ///
    /// object invalid = "abc";
    /// ulong result2 = invalid.ToULongOrDefault(0); // result2 is 0
    /// </code>
    /// </example>
    public static ulong ToULongOrDefault(this object? input, ulong defaultValue = 0) => input.ToULongOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a nullable ulong. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable ulong representation of the input object, or null if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// ulong? result = obj.ToULongOrNull(); // result is 123
    ///
    /// object invalid = "abc";
    /// ulong? result2 = invalid.ToULongOrNull(); // result2 is null
    /// </code>
    /// </example>
    public static ulong? ToULongOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is ulong ulongValue)
        {
            return ulongValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToULongOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="ulong"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted ulong value if successful, or 0 if failed.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToULong(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}"); // Output: Converted: 123
    /// }
    /// </code>
    /// </example>
    public static bool TryToULong(this object? input, out ulong value)
    {
        var r = input.ToULongOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion

    #region UShort Extensions

    /// <summary>
    /// Converts the input object to ushort with a default value.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A ushort representation of the input object, or the default value if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// ushort result = obj.ToUShortOrDefault(0); // result is 123
    ///
    /// object invalid = "abc";
    /// ushort result2 = invalid.ToUShortOrDefault(0); // result2 is 0
    /// </code>
    /// </example>
    public static ushort ToUShortOrDefault(this object? input, ushort defaultValue = 0) => input.ToUShortOrNull() ?? defaultValue;

    /// <summary>
    /// Converts the input object to a nullable ushort. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable ushort representation of the input object, or null if the conversion fails.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// ushort? result = obj.ToUShortOrNull(); // result is 123
    ///
    /// object invalid = "abc";
    /// ushort? result2 = invalid.ToUShortOrNull(); // result2 is null
    /// </code>
    /// </example>
    public static ushort? ToUShortOrNull(this object? input)
    {
        // Performance optimization: check direct type match first
        if (input is ushort ushortValue)
        {
            return ushortValue;
        }

        // Fall back to string conversion for other types
        return input.ToStringOrNull().ToUShortOrNull();
    }

    /// <summary>
    /// 尝试转换为 <see cref="ushort"/>。成功返回 true 并输出值；失败返回 false，输出 0。
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="value">The converted ushort value if successful, or 0 if failed.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <example>
    /// <code>
    /// object obj = "123";
    /// if (obj.TryToUShort(out var result))
    /// {
    ///     Console.WriteLine($"Converted: {result}"); // Output: Converted: 123
    /// }
    /// </code>
    /// </example>
    public static bool TryToUShort(this object? input, out ushort value)
    {
        var r = input.ToUShortOrNull();
        if (r.HasValue)
        {
            value = r.Value;
            return true;
        }
        value = 0;
        return false;
    }

    #endregion
}
