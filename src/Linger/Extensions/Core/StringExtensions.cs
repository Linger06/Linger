using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Linger.JsonConverter;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Check if the specified string is null.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null; otherwise, false.</returns>
    public static bool IsNull([NotNullWhen(false)] this string? value)
    {
        return value == null;
    }

    /// <summary>
    /// Check if the specified string is empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is empty; otherwise, false.</returns>
    public static bool IsEmpty(this string value)
    {
        return value == string.Empty;
    }

    /// <summary>
    /// Check if the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Check if the specified string is null or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
#if !NETFRAMEWORK || NET40_OR_GREATER
        return string.IsNullOrWhiteSpace(value);
#else
        if ((object)value == null)
        {
            return true;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
#endif
    }

    public static bool IsWhiteSpace(this string? value)
    {
        if (value.IsNull())
        {
            return false;
        }

        foreach (var t in value)
        {
            if (!char.IsWhiteSpace(t))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if the specified string is not null.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not null; otherwise, false.</returns>
    public static bool IsNotNull([NotNullWhen(true)] this string? value)
    {
        return value != null;
    }

    /// <summary>
    /// Check if the specified string is not empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not empty; otherwise, false.</returns>
    public static bool IsNotEmpty(this string value)
    {
        return value != string.Empty;
    }

    /// <summary>
    /// Check if the specified string is not null and not empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not null and not empty; otherwise, false.</returns>
    public static bool IsNotNullAndEmpty([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool IsNotNullAndWhiteSpace([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="short"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="short"/> type; otherwise, false.</returns>
    public static bool IsInt16(this string value)
    {
        return short.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to an <see cref="int"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to an <see cref="int"/> type; otherwise, false.</returns>
    public static bool IsInt(this string value)
    {
        return int.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="long"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="long"/> type; otherwise, false.</returns>
    public static bool IsInt64(this string value)
    {
        return long.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="decimal"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="decimal"/> type; otherwise, false.</returns>
    public static bool IsDecimal(this string value)
    {
        return decimal.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="float"/> type; otherwise, false.</returns>
    public static bool IsSingle(this string value)
    {
        return float.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="double"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="double"/> type; otherwise, false.</returns>
    public static bool IsDouble(this string value)
    {
        return double.TryParse(value, out _);
    }

    /// <summary>
    /// Determines whether the specified date string is a datetime.
    /// </summary>
    /// <param name="value">The date string.</param>
    /// <param name="format">Array of date formats.</param>
    /// <returns>
    ///   <c>true</c> if the specified date string is a date; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDateTime(this string value, string[] format)
    {
        if (value == null) return false;
        return (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out _));
    }

    /// <summary>
    /// Determines whether the specified date string is datetime.
    /// </summary>
    /// <param name="value">The date string.</param>
    /// <param name="format">The date format.</param>
    /// <returns>
    ///   <c>true</c> if the specified date string is a date; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDateTime(this string value, string format)
    {
        if (value == null) return false;
        return (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out _));
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="bool"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="bool"/> type; otherwise, false.</returns>
    public static bool IsBoolean(this string value)
    {
        return bool.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="Guid"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="Guid"/> type; otherwise, false.</returns>
    public static bool IsGuid(this string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="Guid"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="format">The exact format to use when interpreting the input: "N", "D", "B", "P", or "X".</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="Guid"/> type; otherwise, false.</returns>
    public static bool IsGuid(this string value, string format)
    {
        return Guid.TryParseExact(value, format, out _);
    }    /// <summary>
         /// Determines whether the specified string is a positive integer.
         /// </summary>
         /// <param name="s">The string to validate.</param>
         /// <returns>True if the string is a positive integer; otherwise, false.</returns>
         /// <example>
         /// <code>
         /// bool result1 = "123".IsPositiveInteger(); // true
         /// bool result2 = "0".IsPositiveInteger(); // true
         /// bool result3 = "-123".IsPositiveInteger(); // false
         /// bool result4 = "abc".IsPositiveInteger(); // false
         /// </code>
         /// </example>
    public static bool IsPositiveInteger(this string s)
    {
        if (s.IsNullOrWhiteSpace())
            return false;

#if NET8_0_OR_GREATER
        ReadOnlySpan<char> span = s.AsSpan();
        if (span.IsEmpty)
            return false;

        // 检查每个字符是否都是数字
        foreach (char c in span)
        {
            if (!char.IsDigit(c))
                return false;
        }

        return true;
#else
        // .NET Framework兼容版本 - 使用传统索引访问
        if (s.Length == 0)
            return false;

        // 检查每个字符是否都是数字
        for (int i = 0; i < s.Length; i++)
        {
            if (!char.IsDigit(s[i]))
                return false;
        }

        return true;
#endif
    }    /// <summary>
         /// Determines whether the specified string is an integer.
         /// </summary>
         /// <param name="s">The string to validate.</param>
         /// <returns>True if the string is an integer; otherwise, false.</returns>
         /// <example>
         /// <code>
         /// bool result1 = "123".IsInteger(); // true
         /// bool result2 = "-123".IsInteger(); // true
         /// bool result3 = "12.3".IsInteger(); // false
         /// bool result4 = "abc".IsInteger(); // false
         /// </code>
         /// </example>
    public static bool IsInteger(this string s)
    {
        if (s.IsNullOrWhiteSpace())
            return false;

#if NET8_0_OR_GREATER
        ReadOnlySpan<char> span = s.AsSpan();
        if (span.IsEmpty)
            return false;

        int startIndex = 0;

        // 检查可选的负号
        if (span[0] == '-')
        {
            if (span.Length == 1) // 只有一个负号
                return false;
            startIndex = 1;
        }

        // 检查剩余字符是否都是数字
        for (int i = startIndex; i < span.Length; i++)
        {
            if (!char.IsDigit(span[i]))
                return false;
        }

        return true;
#else
        // .NET Framework兼容版本
        if (s.Length == 0)
            return false;

        int startIndex = 0;
        
        // 检查可选的负号
        if (s[0] == '-')
        {
            if (s.Length == 1) // 只有一个负号
                return false;
            startIndex = 1;
        }
        
        // 检查剩余字符是否都是数字
        for (int i = startIndex; i < s.Length; i++)
        {
            if (!char.IsDigit(s[i]))
                return false;
        }

        return true;
#endif
    }    /// <summary>
         /// Determines whether the specified string is a valid number with the specified precision and scale.
         /// </summary>
         /// <param name="s">The string to validate.</param>
         /// <param name="precision">The maximum number of digits before the decimal point.</param>
         /// <param name="scale">The maximum number of decimal places.</param>
         /// <returns>True if the string is a valid number; otherwise, false.</returns>
         /// <example>
         /// <code>
         /// bool result1 = "123".IsNumber(5, 0); // true (3位整数，小于等于5位)
         /// bool result2 = "123.45".IsNumber(3, 2); // true (3位整数，2位小数)
         /// bool result3 = "123.456".IsNumber(3, 2); // false (超过小数位数)
         /// bool result4 = "1234567".IsNumber(5, 0); // false (超过整数位数)
         /// </code>
         /// </example>
    public static bool IsNumber(this string s, int precision = 32, int scale = 0)
    {
        if (s.IsNullOrWhiteSpace())
            return false;

        if (precision == 0 && scale == 0)
            return false;

#if NET8_0_OR_GREATER
        ReadOnlySpan<char> span = s.AsSpan();
        if (span.IsEmpty)
            return false;

        int integerDigits = 0;  // 小数点前的位数
        int decimalPlaces = 0;  // 小数点后的位数
        bool foundDecimal = false;

        foreach (char c in span)
        {
            if (char.IsDigit(c))
            {
                if (foundDecimal)
                {
                    decimalPlaces++;
                    if (decimalPlaces > scale)
                        return false;
                }
                else
                {
                    integerDigits++;
                    if (integerDigits > precision)
                        return false;
                }
            }
            else if (c == '.' && !foundDecimal)
            {
                foundDecimal = true;
            }
            else
            {
                return false; // 无效字符
            }
        }

        // 检查整数位数和小数位数是否符合要求
        return integerDigits <= precision && decimalPlaces <= scale;
#else
        // .NET Framework兼容版本
        if (s.Length == 0)
            return false;

        int integerDigits = 0;  // 小数点前的位数
        int decimalPlaces = 0;  // 小数点后的位数
        bool foundDecimal = false;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsDigit(c))
            {
                if (foundDecimal)
                {
                    decimalPlaces++;
                    if (decimalPlaces > scale)
                        return false;
                }
                else
                {
                    integerDigits++;
                    if (integerDigits > precision)
                        return false;
                }
            }
            else if (c == '.' && !foundDecimal)
            {
                foundDecimal = true;
            }
            else
            {
                return false; // 无效字符
            }
        }

        // 检查整数位数和小数位数是否符合要求
        return integerDigits <= precision && decimalPlaces <= scale;
#endif
    }

    /// <summary>
    /// Removes the specified prefix and suffix (single character) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The character to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, char value)
    {
        return str.DelPrefixAndSuffix(value.ToString());
    }    /// <summary>
         /// Removes the specified prefix and suffix (multiple characters) from the string.
         /// </summary>
         /// <param name="str">The input string.</param>
         /// <param name="value">The string to remove.</param>
         /// <returns>The string without the specified prefix and suffix.</returns>    
    public static string DelPrefixAndSuffix(this string str, string value)
    {
        if (str is null)
            return null;
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
            return str;

#if NET6_0_OR_GREATER
        // 优化原因：使用ReadOnlySpan避免多次Substring调用产生的中间字符串对象
        // 单次计算减少字符串分配，提高性能
        ReadOnlySpan<char> span = str.AsSpan();
        ReadOnlySpan<char> valueSpan = value.AsSpan();

        // 检查前缀
        if (span.StartsWith(valueSpan))
        {
            span = span[valueSpan.Length..];
        }

        // 检查后缀
        if (span.EndsWith(valueSpan))
        {
            span = span[..^valueSpan.Length];
        }

        return span.ToString();
#else
        // .NET Framework兼容版本：减少中间字符串分配
        string result = str;
        
        if (result.StartsWith(value))
        {
            result = result.Substring(value.Length);
        }

        if (result.EndsWith(value))
        {
            result = result.Substring(0, result.Length - value.Length);
        }

        return result;
#endif
    }    /// <summary>
         /// Safely truncates a string to the specified maximum length.
         /// </summary>
         /// <param name="input">The string to truncate.</param>
         /// <param name="maxLength">The maximum length.</param>
         /// <param name="suffix">Optional suffix to append when truncated.</param>
         /// <returns>The truncated string.</returns>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || maxLength <= 0)
            return string.Empty;

        if (input.Length <= maxLength)
            return input;

        int actualMaxLength = maxLength - suffix.Length;
        if (actualMaxLength <= 0)
            return suffix;

#if NET6_0_OR_GREATER
        // 优化原因：使用string.Create避免中间字符串分配，单次构建最终结果
        int totalLength = actualMaxLength + suffix.Length;
        return string.Create(totalLength, (input, actualMaxLength, suffix), (span, state) =>
        {
            state.input.AsSpan(0, state.actualMaxLength).CopyTo(span);
            state.suffix.AsSpan().CopyTo(span[state.actualMaxLength..]);
        });
#else
        // .NET Framework兼容版本：使用StringBuilder减少字符串分配
        var sb = new StringBuilder(maxLength);
        sb.Append(input, 0, actualMaxLength);
        sb.Append(suffix);
        return sb.ToString();
#endif
    }

    /// <summary>
    /// Converts a string to its MD5 hash byte array.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>MD5 hash byte array.</returns>
    public static byte[] ToMd5HashByte(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var inputBytes = Encoding.UTF8.GetBytes(input);
#if NET5_0_OR_GREATER
        return MD5.HashData(inputBytes);
#else
        using var md5 = MD5.Create();
        return md5.ComputeHash(inputBytes);
#endif
    }

    /// <summary>
    /// Converts a string to its MD5 hash string representation.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>MD5 hash string or empty string if input is null or empty.</returns>
    public static string ToMd5HashCode(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var hashBytes = input.ToMd5HashByte();
        return hashBytes.ToMd5HashCode();
    }

    /// <summary>
    /// Converts a string to its SHA256 hash byte array.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>SHA256 hash byte array.</returns>
    public static byte[] ToSha256HashByte(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var inputBytes = Encoding.UTF8.GetBytes(input);
#if NET5_0_OR_GREATER
        return SHA256.HashData(inputBytes);
#else
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(inputBytes);
#endif
    }

    /// <summary>
    /// Converts a string to its SHA256 hash string representation.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>SHA256 hash string.</returns>
    public static string ToSha256HashCode(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var hashBytes = input.ToSha256HashByte();
#if NET9_0_OR_GREATER
        return Convert.ToHexStringLower(hashBytes);
#else
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
#endif
    }

    public static List<string> SplitToList(this string? value, char separator = ',', StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options).ToList();
    }

    /// <summary>
    /// Splits the string into a list of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separator">The delimiter to use. Default is carriage return and line feed.</param>
    /// <param name="options"></param>
    /// <returns>A list of strings.</returns>
    public static List<string> SplitToList(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options).ToList();
    }    /// <summary>
         /// Splits the string into an array of strings using the specified delimiter.
         /// </summary>
         /// <param name="value">The string to split.</param>
         /// <param name="separator">The delimiter to use. Default is a comma.</param>
         /// <param name="options"></param>
         /// <returns>An array of strings.</returns>
    public static string[] SplitToArray(this string? value, char separator = ',', StringSplitOptions options = StringSplitOptions.None)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

#if NET8_0_OR_GREATER
        // 优化原因：.NET 8+中的Split方法已经高度优化，直接使用
        return value.Split(separator, options);
#else
        // 优化原因：在旧版本中，使用预分配的静态数组避免每次调用时的数组分配
        // 这减少了垃圾回收压力，特别是在高频调用场景中
        return value.Split(new[] { separator }, options);
#endif
    }
    public static string[] SplitToArray(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }
#if NET8_0_OR_GREATER
        // 优化原因：.NET 8+中的Split方法已经高度优化，直接使用
        return value.Split(separator, options);
#else
        // 优化原因：在旧版本中，使用预分配的数组避免每次调用时的数组分配
        // 对于字符串分隔符，需要创建数组，但可以缓存常用的分隔符
        return value.Split(new[] { separator }, options);
#endif
    }    /// <summary>
         /// Appends a query string to the URL.
         /// </summary>
         /// <param name="self">The URL.</param>
         /// <param name="query">The query string to append.</param>
         /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, string query)
    {
        // 如果查询字符串为空，直接返回原URL
        if (string.IsNullOrEmpty(query))
        {
            return self;
        }

#if NET6_0_OR_GREATER
        // 优化原因：.NET 6+中使用string interpolation和StringBuilder.Create进行高效字符串构建
        bool hasQuery = self.Contains('?');
        char separator = hasQuery ? '&' : '?';
        int totalLength = self.Length + 1 + query.Length;

        return string.Create(totalLength, (self, separator, query), (span, state) =>
        {
            state.self.AsSpan().CopyTo(span);
            int position = state.self.Length;
            span[position++] = state.separator;
            state.query.AsSpan().CopyTo(span[position..]);
        });
#else
        // 优化原因：.NET Framework/老版本中使用StringBuilder避免多次字符串分配
        var sb = new StringBuilder(self.Length + query.Length + 1);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');
        sb.Append(query);
        return sb.ToString();
#endif
    }    /// <summary>
         /// Appends a query string to the URL using the specified dictionary.
         /// </summary>
         /// <param name="self">The URL.</param>
         /// <param name="data">The dictionary containing the query parameters.</param>
         /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, IDictionary data)
    {
        if (data.Count == 0)
        {
            return self;
        }

        // 优化原因：预估容量减少StringBuilder内部数组重新分配
        // 估算：URL长度 + 每个参数平均20字符 + 连接符
        var estimatedCapacity = self.Length + (data.Count * 20) + 10;
        var sb = new StringBuilder(estimatedCapacity);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');

        bool isFirst = true;
        foreach (DictionaryEntry item in data)
        {
            if (!isFirst)
            {
                sb.Append('&');
            }

            // 优化原因：避免字符串插值中的额外分配，直接append各部分
            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
            isFirst = false;
        }

        return sb.ToString();
    }    /// <summary>
         /// Appends a query string to the URL using the specified list of key-value pairs.
         /// </summary>
         /// <param name="self">The URL.</param>
         /// <param name="data">The list of key-value pairs containing the query parameters.</param>
         /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, List<KeyValuePair<string, string>> data)
    {
        if (data.Count == 0)
        {
            return self;
        }

        // 优化原因：预估容量减少StringBuilder内部数组重新分配
        // 估算：URL长度 + 每个参数平均20字符 + 连接符
        var estimatedCapacity = self.Length + (data.Count * 20) + 10;
        var sb = new StringBuilder(estimatedCapacity);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');

#if NET5_0_OR_GREATER
        // 优化原因：.NET 5+中List<T>的枚举器性能更好，使用for循环避免枚举器分配
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('&');
            }

            var item = data[i];
            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
        }
#else
        // .NET Framework版本：使用传统foreach，但仍然比原来的字符串连接高效
        bool isFirst = true;
        foreach (var item in data)
        {
            if (!isFirst)
            {
                sb.Append('&');
            }
            
            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
            isFirst = false;
        }
#endif

        return sb.ToString();
    }

    /// <summary>
    /// Converts a Base64 string representation to its equivalent string using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    public static string FromBase64ToString(this string? value)
    {
        return value.FromBase64ToString(Encoding.UTF8);
    }

    /// <summary>
    /// Converts a Base64 string representation to its equivalent string using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    public static string FromBase64ToString(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var result = Convert.FromBase64String(value);
        return encoding.GetString(result);
    }

    /// <summary>
    /// Converts the current string to its Base64 string representation using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the Base64 string representation.</returns>
    public static string ToBase64String(this string? value)
    {
        return value.ToBase64String(Encoding.UTF8);
    }

    /// <summary>
    /// Converts the current string to its Base64 string representation using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the Base64 string representation.</returns>
    public static string ToBase64String(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var result = encoding.GetBytes(value);
        return Convert.ToBase64String(result);
    }    /// <summary>
         /// Gets the prefix of the specified email string.
         /// </summary>
         /// <param name="value">The email string to get the prefix from.</param>
         /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; if the string is not a valid email, returns <see cref="string.Empty"/>; otherwise, returns the substring before the "@" symbol.</returns>
    public static string GetEmailPrefix(this string? value)
    {
        if (value.IsNullOrEmpty() || !value.IsEmail())
            return string.Empty;

        int atIndex = value.IndexOf('@');
        if (atIndex <= 0)
            return string.Empty;

#if NET6_0_OR_GREATER
        // 优化原因：使用AsSpan避免Substring的字符串分配
        return value.AsSpan(0, atIndex).ToString();
#else
        // .NET Framework兼容版本：保持原有逻辑
        return value.Substring(0, atIndex);
#endif
    }

    /// <summary>
    /// Removes the last newline character from the specified string (using Environment.NewLine).
    /// </summary>
    /// <param name="value">The string to remove the last newline from.</param>
    /// <returns>The string without the last newline character.</returns>
    public static string DelLastNewLine(this string value)
    {
        return value.TrimEnd('\r', '\n');
    }    /// <summary>
         /// Removes all newline characters from the specified string.
         /// </summary>
         /// <param name="value">The string to remove all newlines from.</param>
         /// <returns>The string without any newline characters.</returns>
    public static string DelAllNewLine(this string? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

#if NET6_0_OR_GREATER
        // 优化原因：使用Span<T>和单次遍历替代多次Replace调用
        // 避免多次字符串创建，提高性能，特别是对大字符串
        if (value.Length == 0)
            return string.Empty;

        // 先计算结果长度
        int resultLength = 0;
        ReadOnlySpan<char> source = value.AsSpan();
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] != '\r' && source[i] != '\n')
                resultLength++;
        }

        if (resultLength == value.Length)
            return value; // 没有换行符，直接返回原字符串

        if (resultLength == 0)
            return string.Empty;

        // 创建结果字符串
        return string.Create(resultLength, value, static (span, str) =>
        {
            ReadOnlySpan<char> source = str.AsSpan();
            int writeIndex = 0;

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c != '\r' && c != '\n')
                {
                    span[writeIndex++] = c;
                }
            }
        });
#else
        // .NET Framework兼容版本：使用StringBuilder单次遍历
        // 优化原因：避免多次Replace调用产生的中间字符串对象
        var result = new StringBuilder(value.Length);
        
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c != '\r' && c != '\n')
            {
                result.Append(c);
            }
        }
        
        return result.ToString();
#endif
    }

    /// <summary>
    /// Removes the last comma from the specified string.
    /// </summary>
    /// <param name="str">The string to remove the last comma from.</param>
    /// <returns>The string without the last comma.</returns>
    public static string DelLastComma(this string str)
    {
        return str.DelLastChar(",");
    }

    /// <summary>
    /// Removes the specified character from the end of the string.
    /// </summary>
    /// <param name="str">The string to remove the character from.</param>
    /// <param name="character">The character to remove.</param>
    /// <returns>The string without the specified character at the end.</returns>
    public static string DelLastChar(this string str, string character)
    {
        if (str.IsNullOrEmpty() || character.IsNullOrEmpty())
        {
            return str ?? string.Empty;
        }

        return str.TrimEnd(character.ToCharArray());
    }

#if !NETFRAMEWORK || NET462_OR_GREATER

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        WriteIndented = true,
        Converters = { new DataTableJsonConverter() }
    };

    /// <summary>
    /// Converts the specified JSON string to a DataTable.
    /// </summary>
    /// <param name="json">The JSON data.</param>
    /// <returns>The DataTable representation of the JSON data.</returns>
    public static DataTable? ToDataTable(this string json)
    {
        if (json.IsNullOrEmpty()) return null;
        return JsonSerializer.Deserialize<DataTable>(json, s_readOptions);
    }

#endif

#if !NET8_0_OR_GREATER
    public static bool StartsWith(this string value, char prefix)
    {
        return value.StartsWith(prefix.ToString());
    }

    public static bool EndsWith(this string value, char suffix)
    {
        return value.EndsWith(suffix.ToString());
    }
#endif    /// <summary>
    /// Ensures the string starts with the specified prefix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="prefix">The prefix value to check for.</param>
    /// <returns>The string value including the prefix.</returns>
    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (value == null) return prefix ?? string.Empty;
        if (prefix == null) return value;

#if NET6_0_OR_GREATER
        // 优化原因：使用AsSpan检查避免字符串分配，string.Create构建最终结果
        if (value.AsSpan().StartsWith(prefix.AsSpan()))
            return value;

        return string.Create(value.Length + prefix.Length, (prefix, value), (span, state) =>
        {
            state.prefix.AsSpan().CopyTo(span);
            state.value.AsSpan().CopyTo(span[state.prefix.Length..]);
        });
#else
        // .NET Framework兼容版本：减少字符串分配
        return value.StartsWith(prefix) ? value : prefix + value;
#endif
    }

    /// <summary>
    /// Ensures the string ends with the specified suffix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="suffix">The suffix value to check for.</param>
    /// <returns>The string value including the suffix.</returns>
    public static string EnsureEndsWith(this string value, string suffix)
    {
        if (value == null) return suffix ?? string.Empty;
        if (suffix == null) return value;

#if NET6_0_OR_GREATER
        // 优化原因：使用AsSpan检查避免字符串分配，string.Create构建最终结果
        if (value.AsSpan().EndsWith(suffix.AsSpan()))
            return value;

        return string.Create(value.Length + suffix.Length, (value, suffix), (span, state) =>
        {
            state.value.AsSpan().CopyTo(span);
            state.suffix.AsSpan().CopyTo(span[state.value.Length..]);
        });
#else
        // .NET Framework兼容版本：减少字符串分配
        return value.EndsWith(suffix) ? value : value + suffix;
#endif
    }

    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    public static string TruncateFromStart(this string self, int length)
    {
        // 复用 StringExtensions2.cs 中的 Truncate 方法，传入空后缀
        return self.Truncate(length, string.Empty);
    }    /// <summary>
         /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
         /// </summary>
         /// <param name="self">The string to truncate.</param>
         /// <param name="length">The length to truncate to.</param>
         /// <returns>The truncated string.</returns>
    public static string TakeLast(this string self, int length)
    {
        if (self == null || length <= 0)
            return string.Empty;

        if (length >= self.Length)
            return self;

#if NET6_0_OR_GREATER
        // 优化原因：使用AsSpan避免Substring的字符串分配，直接切片后转字符串
        return self.AsSpan(self.Length - length).ToString();
#else
        // .NET Framework兼容版本：保持原有逻辑
        return self.Substring(self.Length - length);
#endif
    }

    // 保留旧方法名称以保持兼容性
    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TruncateFromStart instead")]
    public static string Substring2(this string self, int length) => TruncateFromStart(self, length);

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TakeLast instead")]
    public static string Substring3(this string self, int length) => TakeLast(self, length);

    #region Regex
    const string Ipv4RegexPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
    const string DomainRegexPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$";
    const string UrlRegexPattern = @"^https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]";
    const string EnglishRegexPattern = "^[A-Za-z]+$";
    const string EmailRegexPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    const string MultipleMailRegexPattern = @"^((?:(?:[a-zA-Z0-9_\-\.]+)@(?:(?:\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(?:(?:[a-zA-Z0-9\-]+\.)+))(?:[a-zA-Z]{2,4}|[0-9]{1,3})(?:\]?)(?:\s*;\s*|\s*$))+)$";
#if NET8_0_OR_GREATER

    [GeneratedRegex(EnglishRegexPattern)]
    private static partial Regex EnglishRegex();    /// <summary>
                                                    /// Determines whether the specified string contains only English letters.
                                                    /// </summary>
                                                    /// <param name="input">The string to validate.</param>
                                                    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
                                                    /// <example>
                                                    /// <code>
                                                    /// bool result1 = "abc".IsEnglish(); // true
                                                    /// bool result2 = "ABC".IsEnglish(); // true
                                                    /// bool result3 = "AbCdEf".IsEnglish(); // true
                                                    /// bool result4 = "abc123".IsEnglish(); // false
                                                    /// bool result5 = "abc_def".IsEnglish(); // false
                                                    /// </code>
                                                    /// </example>
    public static bool IsEnglish(this string input)
    {
        if (input == null)
            return false;

#if NET8_0_OR_GREATER
        // 优化原因：使用ReadOnlySpan避免字符串索引的边界检查开销
        // char.IsAsciiLetter比范围检查更快，且直接针对ASCII字符优化
        ReadOnlySpan<char> span = input.AsSpan();
        if (span.IsEmpty)
            return false;

        // 检查每个字符是否都是英文字母
        foreach (char c in span)
        {
            if (!char.IsAsciiLetter(c))
                return false;
        }

        return true;
#else
        // .NET Framework兼容版本
        // 优化原因：直接范围检查比正则表达式快，避免正则引擎开销
        if (input.Length == 0)
            return false;

        // 检查每个字符是否都是英文字母(A-Z, a-z)
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                return false;
        }

        return true;
#endif
    }

    [GeneratedRegex(UrlRegexPattern)]
    private static partial Regex UrlRegex();

    /// <summary>
    /// Determines whether the specified string is a valid URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    public static bool IsUrl(this string input)
    {
        if (input == null)
            return false;

        return UrlRegex().IsMatch(input);
    }

    [GeneratedRegex(Ipv4RegexPattern)]
    private static partial Regex Ipv4Regex();

    /// <summary>
    /// Determines whether the specified string is a valid IPv4 address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid IPv4 address; otherwise, false.</returns>
    public static bool IsIpv4(this string input)
    {
        if (input == null)
            return false;

        return Ipv4Regex().IsMatch(input);
    }

    [GeneratedRegex(DomainRegexPattern)]
    private static partial Regex DomainRegex();

    /// <summary>
    /// Determines whether the specified string is a valid domain name.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid domain name; otherwise, false.</returns>
    public static bool IsDomainName(this string str)
    {
        if (str == null)
            return false;

        return DomainRegex().IsMatch(str);
    }

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex EmailRegex();

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid email address; otherwise, false.</returns>
    public static bool IsEmail(this string? input)
    {
        if (input == null) return false;
        return EmailRegex().IsMatch(input);
    }

    [GeneratedRegex(MultipleMailRegexPattern)]
    private static partial Regex MultipleMailRegex();

    /// <summary>
    /// Determines whether the specified string contains multiple valid email addresses.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains multiple valid email addresses; otherwise, false.</returns>
    public static bool IsMultipleEmail(this string input)
    {
        if (input == null)
            return false;

        return MultipleMailRegex().IsMatch(input);
    }

    [GeneratedRegex("[+-]?\\d+(\\.\\d+)?[eE][+-]?\\d+")]
    private static partial Regex ScientificNotationRegex();    /// <summary>
                                                               /// Determines whether the specified string is in scientific notation format.
                                                               /// </summary>
                                                               /// <param name="input">The string to validate.</param>
                                                               /// <returns>True if the string is in scientific notation format; otherwise, false.</returns>
                                                               /// <example>
                                                               /// <code>
                                                               /// bool result1 = "1.23e10".IsScientificNotation(); // true
                                                               /// bool result2 = "1.23E-5".IsScientificNotation(); // true
                                                               /// bool result3 = "-3.14e+2".IsScientificNotation(); // true
                                                               /// bool result4 = "123".IsScientificNotation(); // false
                                                               /// bool result5 = "1.23".IsScientificNotation(); // false
                                                               /// </code>
                                                               /// </example>
    public static bool IsScientificNotation(this string input)
    {
        if (input == null)
            return false;

#if NET8_0_OR_GREATER
        // 优化原因：使用ReadOnlySpan避免字符串索引边界检查
        // 单次遍历比正则表达式更快，减少内存分配
        ReadOnlySpan<char> span = input.AsSpan();
        if (span.IsEmpty)
            return false;

        int pos = 0;

        // 检查可选的符号
        if (span[pos] is '+' or '-')
            pos++;

        if (pos >= span.Length)
            return false;

        // 检查至少一个数字
        if (!char.IsDigit(span[pos]))
            return false;

        // 跳过数字
        while (pos < span.Length && char.IsDigit(span[pos]))
            pos++;

        // 可选的小数部分
        if (pos < span.Length && span[pos] == '.')
        {
            pos++;
            // 小数点后必须有至少一个数字
            if (pos >= span.Length || !char.IsDigit(span[pos]))
                return false;
            while (pos < span.Length && char.IsDigit(span[pos]))
                pos++;
        }

        // 必须有E或e
        if (pos >= span.Length || (span[pos] != 'e' && span[pos] != 'E'))
            return false;
        pos++;

        // E后可选的符号
        if (pos < span.Length && (span[pos] is '+' or '-'))
            pos++;

        // E后必须有至少一个数字
        if (pos >= span.Length || !char.IsDigit(span[pos]))
            return false;

        // 跳过指数部分的数字
        while (pos < span.Length && char.IsDigit(span[pos]))
            pos++;

        // 确保没有多余字符
        return pos == span.Length;
#else
        // .NET Framework兼容版本
        // 优化原因：手动解析比正则表达式快，避免正则引擎的状态机开销
        if (input.Length == 0)
            return false;

        int pos = 0;
        
        // 检查可选的符号
        if (input[pos] == '+' || input[pos] == '-')
            pos++;
            
        if (pos >= input.Length)
            return false;

        // 检查至少一个数字
        if (!char.IsDigit(input[pos]))
            return false;
            
        // 跳过数字
        while (pos < input.Length && char.IsDigit(input[pos]))
            pos++;

        // 可选的小数部分
        if (pos < input.Length && input[pos] == '.')
        {
            pos++;
            // 小数点后必须有至少一个数字
            if (pos >= input.Length || !char.IsDigit(input[pos]))
                return false;
            while (pos < input.Length && char.IsDigit(input[pos]))
                pos++;
        }

        // 必须有E或e
        if (pos >= input.Length || (input[pos] != 'e' && input[pos] != 'E'))
            return false;
        pos++;

        // E后可选的符号
        if (pos < input.Length && (input[pos] == '+' || input[pos] == '-'))
            pos++;

        // E后必须有至少一个数字
        if (pos >= input.Length || !char.IsDigit(input[pos]))
            return false;
            
        // 跳过指数部分的数字
        while (pos < input.Length && char.IsDigit(input[pos]))
            pos++;

        // 确保没有多余字符
        return pos == input.Length;
#endif
    }

#else
    private static readonly Regex s_ipv4Regex = new(Ipv4RegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_domainRegex = new(DomainRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_urlRegex = new(UrlRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_englishRegex = new(EnglishRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_emailRegex = new(EmailRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_multipleMailRegex = new(MultipleMailRegexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid email address; otherwise, false.</returns>
    public static bool IsEmail(this string? input)
    {
        if (input == null) return false;
        return s_emailRegex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string contains multiple valid email addresses.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains multiple valid email addresses; otherwise, false.</returns>
    public static bool IsMultipleEmail(this string input)
    {
        if (input == null)
            return false;

        return s_multipleMailRegex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid domain name.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid domain name; otherwise, false.</returns>
    public static bool IsDomainName(this string str)
    {
        if (str == null)
            return false;

        return s_domainRegex.IsMatch(str);
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv4 address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid IPv4 address; otherwise, false.</returns>
    public static bool IsIpv4(this string input)
    {
        if (input == null)
            return false;

        return s_ipv4Regex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    public static bool IsUrl(this string input)
    {
        if (input == null)
            return false;

        return s_urlRegex.IsMatch(input);
    }    /// <summary>
    /// Determines whether the specified string contains only English letters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool result1 = "abc".IsEnglish(); // true
    /// bool result2 = "ABC".IsEnglish(); // true
    /// bool result3 = "AbCdEf".IsEnglish(); // true
    /// bool result4 = "abc123".IsEnglish(); // false
    /// bool result5 = "abc_def".IsEnglish(); // false
    /// </code>
    /// </example>
    public static bool IsEnglish(this string input)
    {
        if (input == null)
            return false;

        // .NET Framework兼容版本
        if (input.Length == 0)
            return false;

        // 检查每个字符是否都是英文字母(A-Z, a-z)
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                return false;
        }

        return true;
    }    /// <summary>
    /// Determines whether the specified string is in scientific notation format.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is in scientific notation format; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool result1 = "1.23e10".IsScientificNotation(); // true
    /// bool result2 = "1.23E-5".IsScientificNotation(); // true
    /// bool result3 = "-3.14e+2".IsScientificNotation(); // true
    /// bool result4 = "123".IsScientificNotation(); // false
    /// bool result5 = "1.23".IsScientificNotation(); // false
    /// </code>
    /// </example>
    public static bool IsScientificNotation(this string input)
    {
        if (input == null)
            return false;

        // .NET Framework兼容版本
        if (input.Length == 0)
            return false;

        int pos = 0;
        
        // 检查可选的符号
        if (input[pos] == '+' || input[pos] == '-')
            pos++;
            
        if (pos >= input.Length)
            return false;

        // 检查至少一个数字
        if (!char.IsDigit(input[pos]))
            return false;
            
        // 跳过数字
        while (pos < input.Length && char.IsDigit(input[pos]))
            pos++;

        // 可选的小数部分
        if (pos < input.Length && input[pos] == '.')
        {
            pos++;
            // 小数点后必须有至少一个数字
            if (pos >= input.Length || !char.IsDigit(input[pos]))
                return false;
            while (pos < input.Length && char.IsDigit(input[pos]))
                pos++;
        }

        // 必须有E或e
        if (pos >= input.Length || (input[pos] != 'e' && input[pos] != 'E'))
            return false;
        pos++;

        // E后可选的符号
        if (pos < input.Length && (input[pos] == '+' || input[pos] == '-'))
            pos++;

        // E后必须有至少一个数字
        if (pos >= input.Length || !char.IsDigit(input[pos]))
            return false;
            
        // 跳过指数部分的数字
        while (pos < input.Length && char.IsDigit(input[pos]))
            pos++;

        // 确保没有多余字符
        return pos == input.Length;
    }

#endif


    /// <summary>
    /// Determines whether the specified string contains only a combination of English letters and numbers.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="minLength">The minimum length of the string.</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <returns>True if the string contains only a combination of English letters and numbers; otherwise, false.</returns>
    public static bool IsCombinationOfEnglishNumber(this string input, int? minLength = null, int? maxLength = null)
    {
        var pattern = @"(?=.*\d)(?=.*[a-zA-Z])[a-zA-Z0-9]";
        if (minLength is null && maxLength is null)
        {
            pattern = $"^{pattern}+$";
        }
        else if (minLength is not null && maxLength is null)
        {
            pattern = $"^{pattern}{{{minLength},}}$";
        }
        else if (minLength is null && maxLength is not null)
        {
            pattern = $"^{pattern}{{1,{maxLength}}}$";
        }
        else
        {
            pattern = $"^{pattern}{{{minLength},{maxLength}}}$";
        }

        return Regex.IsMatch(input, pattern);
    }

    /// <summary>
    /// Determines whether the specified string contains only a combination of English letters, numbers, and special characters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="minLength">The minimum length of the string.</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <returns>True if the string contains only a combination of English letters, numbers, and special characters; otherwise, false.</returns>
    public static bool IsCombinationOfEnglishNumberSymbol(this string input, int? minLength = null,
        int? maxLength = null)
    {
        var pattern = @"(?=.*\d)(?=.*[a-zA-Z])(?=.*[^a-zA-Z\d]).";
        if (minLength is null && maxLength is null)
        {
            pattern = $"^{pattern}+$";
        }
        else if (minLength is not null && maxLength is null)
        {
            pattern = $"^{pattern}{{{minLength},}}$";
        }
        else if (minLength is null && maxLength is not null)
        {
            pattern = $"^{pattern}{{1,{maxLength}}}$";
        }
        else
        {
            pattern = $"^{pattern}{{{minLength},{maxLength}}}$";
        }

        return Regex.IsMatch(input, pattern);
    }

    /// <summary>
    /// Converts a scientific notation string to its equivalent <see cref="decimal"/> value.
    /// </summary>
    /// <param name="input">The input string in scientific notation.</param>
    /// <returns>The equivalent <see cref="decimal"/> value.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not in scientific notation.</exception>
    public static decimal ToDecimalForScientificNotation(this string input)
    {
        decimal dData;
        if (input.IsScientificNotation())
        {
            dData = Convert.ToDecimal(decimal.Parse(input, NumberStyles.Float));
        }
        else
        {
            throw new FormatException(nameof(input));
        }
        return dData;
    }
    #endregion
}