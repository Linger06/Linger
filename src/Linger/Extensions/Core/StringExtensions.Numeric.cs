using System.Globalization;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="short"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="short"/> type; otherwise, false.</returns>
    public static bool IsInt16(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return short.TryParse(value.AsSpan(), out _);
#else
        return short.TryParse(value, out _);
#endif
    }

    /// <summary>
    /// Check if the specified string is equivalent to an <see cref="int"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to an <see cref="int"/> type; otherwise, false.</returns>
    public static bool IsInt(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return int.TryParse(value.AsSpan(), out _);
#else
        return int.TryParse(value, out _);
#endif
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="long"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="long"/> type; otherwise, false.</returns>
    public static bool IsInt64(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return long.TryParse(value.AsSpan(), out _);
#else
        return long.TryParse(value, out _);
#endif
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="decimal"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="decimal"/> type; otherwise, false.</returns>
    public static bool IsDecimal(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return decimal.TryParse(value.AsSpan(), out _);
#else
        return decimal.TryParse(value, out _);
#endif
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="float"/> type; otherwise, false.</returns>
    public static bool IsSingle(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return float.TryParse(value.AsSpan(), out _);
#else
        return float.TryParse(value, out _);
#endif
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="double"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="double"/> type; otherwise, false.</returns>
    public static bool IsDouble(this string? value)
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return double.TryParse(value.AsSpan(), out _);
#else
        return double.TryParse(value, out _);
#endif
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
    }

    /// <summary>
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
    }

    /// <summary>
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
    }

    /// <summary>
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
}
