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
    }

    /// <summary>
    /// Determines whether the specified string is a positive integer.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <returns>True if the string is a positive integer; otherwise, false.</returns>
    public static bool IsPositiveInteger(this string s)
    {
        if (s.IsNullOrWhiteSpace()) return false;

        var pattern = @"^\d*$";
        return Regex.IsMatch(s, pattern);
    }

    /// <summary>
    /// Determines whether the specified string is an integer.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <returns>True if the string is an integer; otherwise, false.</returns>
    public static bool IsInteger(this string s)
    {
        var pattern = @"^-?\d+$";
        return Regex.IsMatch(s, pattern);
    }

    /// <summary>
    /// Determines whether the specified string is a valid number with the specified precision and scale.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <param name="precision">The maximum number of digits.</param>
    /// <param name="scale">The maximum number of decimal places.</param>
    /// <returns>True if the string is a valid number; otherwise, false.</returns>
    public static bool IsNumber(this string s, int precision = 32, int scale = 0)
    {
        if (precision == 0 && scale == 0)
        {
            return false;
        }

        var pattern = @"(^\d{1," + precision + "}";
        if (scale > 0)
        {
            pattern += @"\.\d{0," + scale + "}$)|" + pattern;
        }

        pattern += "$)";
        return Regex.IsMatch(s, pattern);
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
    }

    /// <summary>
    /// Removes the specified prefix and suffix (multiple characters) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The string to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, string value)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
            return str;

        if (str.StartsWith(value))
        {
            str = str.Substring(value.Length);
        }

        if (str.EndsWith(value))
        {
            str = str.Substring(0, str.LastIndexOf(value, StringComparison.Ordinal));
        }

        return str;
    }

    /// <summary>
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

        return $"{input.Substring(0, actualMaxLength)}{suffix}";
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
    }

    /// <summary>
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
        return value.Split(separator, options);
#else
        return value.Split([separator], options);
#endif
    }

    public static string[] SplitToArray(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }
#if NET8_0_OR_GREATER
        return value.Split(separator, options);
#else
        return value.Split([separator], options);
#endif
    }

    /// <summary>
    /// Appends a query string to the URL.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="query">The query string to append.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, string query)
    {
        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
    }

    /// <summary>
    /// Appends a query string to the URL using the specified dictionary.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The dictionary containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, IDictionary data)
    {
        var query = string.Empty;
        foreach (DictionaryEntry item in data)
        {
            query += item.Key + "=" + item.Value + "&";
        }

        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
    }

    /// <summary>
    /// Appends a query string to the URL using the specified list of key-value pairs.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The list of key-value pairs containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, List<KeyValuePair<string, string>> data)
    {
        var query = string.Empty;
        foreach (KeyValuePair<string, string> item in data)
        {
            query += item.Key + "=" + item.Value + "&";
        }

        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
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
    }

    /// <summary>
    /// Gets the prefix of the specified email string.
    /// </summary>
    /// <param name="value">The email string to get the prefix from.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; if the string is not a valid email, returns <see cref="string.Empty"/>; otherwise, returns the substring before the "@" symbol.</returns>
    public static string GetEmailPrefix(this string? value)
    {
        if (value.IsNullOrEmpty() || !value.IsEmail())
        {
            return string.Empty;
        }

        int atIndex = value.IndexOf('@');
        return atIndex > 0 ? value.Substring(0, atIndex) : string.Empty;
    }

    /// <summary>
    /// Removes the last newline character from the specified string (using Environment.NewLine).
    /// </summary>
    /// <param name="value">The string to remove the last newline from.</param>
    /// <returns>The string without the last newline character.</returns>
    public static string DelLastNewLine(this string value)
    {
        return value.TrimEnd('\r', '\n');
    }

    /// <summary>
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

        return value.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
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
#endif

    /// <summary>
    /// Ensures the string starts with the specified prefix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="prefix">The prefix value to check for.</param>
    /// <returns>The string value including the prefix.</returns>
    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (value == null) return prefix ?? string.Empty;
        if (prefix == null) return value;

        return value.StartsWith(prefix) ? value : string.Concat(prefix, value);
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

        return value.EndsWith(suffix) ? value : string.Concat(value, suffix);
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
    }

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    public static string TakeLast(this string self, int length)
    {
        if (self == null || length <= 0) return string.Empty;

        if (length >= self.Length)
        {
            return self;
        }

        return self.Substring(self.Length - length);
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
    private static partial Regex EnglishRegex();

    /// <summary>
    /// Determines whether the specified string contains only English letters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
    public static bool IsEnglish(this string input)
    {
        if (input == null)
            return false;

        return EnglishRegex().IsMatch(input);
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
    private static partial Regex ScientificNotationRegex();

    // 判断是否是科学计数法
    public static bool IsScientificNotation(this string input)
    {
        if (input == null)
            return false;

        return ScientificNotationRegex().IsMatch(input);
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
    }

    /// <summary>
    /// Determines whether the specified string contains only English letters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
    public static bool IsEnglish(this string input)
    {
        if (input == null)
            return false;

        return s_englishRegex.IsMatch(input);
    }

    // 判断是否是科学计数法
    public static bool IsScientificNotation(this string input)
    {
        if (input == null)
            return false;

        return Regex.IsMatch(input, "[+-]?\\d+(\\.\\d+)?[eE][+-]?\\d+");
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