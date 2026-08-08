namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
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
    /// Determines whether the specified string can be converted to a <see cref="short"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsShort(this string? value) => value.TryToShort(out _);

    /// <summary>
    /// Determines whether the specified string can be converted to an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsInt(this string? value) => value.TryToInt(out _);

    /// <summary>
    /// Determines whether the specified string can be converted to a <see cref="long"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsLong(this string? value) => value.TryToLong(out _);

    /// <summary>
    /// Determines whether the specified string can be converted to a <see cref="decimal"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsDecimal(this string? value) => value.TryToDecimal(out _);

    /// <summary>
    /// Determines whether the specified string can be converted to a <see cref="float"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsFloat(this string? value) => value.TryToFloat(out _);

    /// <summary>
    /// Determines whether the specified string can be converted to a <see cref="double"/>.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool IsDouble(this string? value) => value.TryToDouble(out _);

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
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
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
        {
            return false;
        }

        var startIndex = 0;

        if (s[0] == '-')
        {
            if (s.Length == 1)
            {
                return false;
            }

            startIndex = 1;
        }

        for (var i = startIndex; i < s.Length; i++)
        {
            if (!char.IsDigit(s[i]))
            {
                return false;
            }
        }

        return true;
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
        {
            return false;
        }

        if (precision == 0 && scale == 0)
        {
            return false;
        }

        var integerDigits = 0;
        var decimalPlaces = 0;
        var foundDecimal = false;

        foreach (var c in s)
        {
            if (char.IsDigit(c))
            {
                if (foundDecimal)
                {
                    decimalPlaces++;
                    if (decimalPlaces > scale)
                    {
                        return false;
                    }
                }
                else
                {
                    integerDigits++;
                    if (integerDigits > precision)
                    {
                        return false;
                    }
                }
            }
            else if (c == '.' && !foundDecimal)
            {
                foundDecimal = true;
            }
            else
            {
                return false;
            }
        }

        if (integerDigits + decimalPlaces == 0)
        {
            return false;
        }

        if (foundDecimal && (scale == 0 || decimalPlaces == 0))
        {
            return false;
        }

        return integerDigits <= precision && decimalPlaces <= scale;
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
        if (input is null)
        {
            return false;
        }

        if (input.Length == 0)
        {
            return false;
        }

        var pos = 0;

        if (input[pos] is '+' or '-')
        {
            pos++;
        }

        if (pos >= input.Length)
        {
            return false;
        }

        if (!char.IsDigit(input[pos]))
        {
            return false;
        }

        while (pos < input.Length && char.IsDigit(input[pos]))
        {
            pos++;
        }

        if (pos < input.Length && input[pos] == '.')
        {
            pos++;
            if (pos >= input.Length || !char.IsDigit(input[pos]))
            {
                return false;
            }

            while (pos < input.Length && char.IsDigit(input[pos]))
            {
                pos++;
            }
        }

        if (pos >= input.Length || (input[pos] != 'e' && input[pos] != 'E'))
        {
            return false;
        }

        pos++;

        if (pos < input.Length && (input[pos] is '+' or '-'))
        {
            pos++;
        }

        if (pos >= input.Length || !char.IsDigit(input[pos]))
        {
            return false;
        }

        while (pos < input.Length && char.IsDigit(input[pos]))
        {
            pos++;
        }

        return pos == input.Length;
    }

    /// <summary>
    /// Converts a scientific notation string to its equivalent <see cref="decimal"/> value.
    /// </summary>
    /// <param name="input">The input string in scientific notation.</param>
    /// <returns>The equivalent <see cref="decimal"/> value.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not in scientific notation.</exception>
    public static decimal ToDecimalForScientificNotation(this string input)
    {
        if (!input.IsScientificNotation())
        {
            throw new FormatException(nameof(input));
        }

        return decimal.Parse(input, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
