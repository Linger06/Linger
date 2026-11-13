namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Gets the prefix of the specified email string.
    /// </summary>
    /// <param name="value">The email string to get the prefix from.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; if the string is not a valid email, returns <see cref="string.Empty"/>; otherwise, returns the substring before the "@" symbol.</returns>
    public static string GetEmailPrefix(this string? value)
    {
        if (value.IsNullOrEmpty() || !value.IsEmail())
            return string.Empty;

        var atIndex = value.IndexOf('@');
        if (atIndex <= 0)
            return string.Empty;

#if NET6_0_OR_GREATER
        return value.AsSpan(0, atIndex).ToString();
#else
        return value.Substring(0, atIndex);
#endif
    }

#if !NET8_0_OR_GREATER
    /// <summary>判断字符串首字符是否等于指定字符。</summary>
    /// <param name="value">要检查的字符串。</param>
    /// <param name="prefix">要匹配的首字符。</param>
    /// <returns>字符串非空且首字符相等时返回 true。</returns>
    public static bool StartsWith(this string value, char prefix)
    {
        return value.Length > 0 && value[0] == prefix;
    }

    /// <summary>判断字符串末字符是否等于指定字符。</summary>
    /// <param name="value">要检查的字符串。</param>
    /// <param name="suffix">要匹配的末字符。</param>
    /// <returns>字符串非空且末字符相等时返回 true。</returns>
    public static bool EndsWith(this string value, char suffix)
    {
        return value.Length > 0 && value[value.Length - 1] == suffix;
    }
#endif
}
