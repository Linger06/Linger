using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    #region Regex Constants
#if !NET10_0_OR_GREATER
    private const string Ipv4RegexPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
#endif
    private const string DomainRegexPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$";
    private const string UrlRegexPattern = @"^https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]";
    private const string EmailRegexPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    private const string MultipleMailRegexPattern = @"^((?:(?:[a-zA-Z0-9_\-\.]+)@(?:(?:\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(?:(?:[a-zA-Z0-9\-]+\.)+))(?:[a-zA-Z]{2,4}|[0-9]{1,3})(?:\]?)(?:\s*;\s*|\s*$))+)$";
    #endregion

#if NET8_0_OR_GREATER
    [GeneratedRegex(UrlRegexPattern)]
    private static partial Regex GeneratedUrlRegex();

    [GeneratedRegex(DomainRegexPattern)]
    private static partial Regex GeneratedDomainRegex();

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex GeneratedEmailRegex();

    [GeneratedRegex(MultipleMailRegexPattern)]
    private static partial Regex GeneratedMultipleMailRegex();

#if !NET10_0_OR_GREATER
    [GeneratedRegex(Ipv4RegexPattern)]
    private static partial Regex GeneratedIpv4Regex();

    private static Regex GetIpv4Regex() => GeneratedIpv4Regex();
#endif

    private static Regex GetUrlRegex() => GeneratedUrlRegex();

    private static Regex GetDomainRegex() => GeneratedDomainRegex();

    private static Regex GetEmailRegex() => GeneratedEmailRegex();

    private static Regex GetMultipleMailRegex() => GeneratedMultipleMailRegex();
#else
    private static readonly Regex s_ipv4Regex = new(Ipv4RegexPattern, RegexOptions.Compiled);
    private static readonly Regex s_domainRegex = new(DomainRegexPattern, RegexOptions.Compiled);
    private static readonly Regex s_urlRegex = new(UrlRegexPattern, RegexOptions.Compiled);
    private static readonly Regex s_emailRegex = new(EmailRegexPattern, RegexOptions.Compiled);
    private static readonly Regex s_multipleMailRegex = new(MultipleMailRegexPattern, RegexOptions.Compiled);

    private static Regex GetIpv4Regex() => s_ipv4Regex;

    private static Regex GetUrlRegex() => s_urlRegex;

    private static Regex GetDomainRegex() => s_domainRegex;

    private static Regex GetEmailRegex() => s_emailRegex;

    private static Regex GetMultipleMailRegex() => s_multipleMailRegex;
#endif

    private static bool IsIpAddressCore(string input)
    {
#if NET10_0_OR_GREATER
        return System.Net.IPAddress.IsValid(input);
#else
        return System.Net.IPAddress.TryParse(input, out _);
#endif
    }

    private static bool IsIpv4Core(string input)
    {
#if NET10_0_OR_GREATER
        if (!System.Net.IPAddress.TryParse(input, out var ip)
            || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return false;
        }

        var dotCount = 0;
        foreach (var character in input)
        {
            if (character == '.')
            {
                dotCount++;
            }
        }

        return dotCount == 3;
#else
        return GetIpv4Regex().IsMatch(input);
#endif
    }

    private static bool IsAsciiLetter(char character)
    {
#if NET8_0_OR_GREATER
        return char.IsAsciiLetter(character);
#else
        return (character >= 'A' && character <= 'Z')
            || (character >= 'a' && character <= 'z');
#endif
    }

    /// <summary>
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
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (var character in input)
        {
            if (!IsAsciiLetter(character))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified string is a valid URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok1 = "https://example.com".IsUrl();    // true
    /// bool ok2 = "http://localhost:8080".IsUrl();  // true
    /// bool no1 = "ftp://example.com".IsUrl();      // false
    /// </code>
    /// </example>
    public static bool IsUrl(this string input)
    {
        return input is not null && GetUrlRegex().IsMatch(input);
    }

    /// <summary>
    /// 判断指定字符串是否为有效的 IP 地址（IPv4 或 IPv6）。
    /// </summary>
    /// <param name="input">要验证的字符串。</param>
    /// <returns>如果字符串是有效的 IP 地址，则返回 true；否则返回 false。</returns>
    /// <example>
    /// <code>
    /// bool ok1 = "192.168.1.1".IsIpAddress();              // true (IPv4)
    /// bool ok2 = "::1".IsIpAddress();                       // true (IPv6)
    /// bool ok3 = "2001:db8::1".IsIpAddress();              // true (IPv6)
    /// bool no = "999.1.1.1".IsIpAddress();                  // false
    /// </code>
    /// </example>
    public static bool IsIpAddress(this string? input)
    {
        return input is not null && IsIpAddressCore(input);
    }

    /// <summary>
    /// 判断指定字符串是否为有效的 IPv4 地址。
    /// 注意：只接受标准的点分十进制格式（如 192.168.1.1），不接受简写形式（如 192.168.1）。
    /// </summary>
    /// <param name="input">要验证的字符串。</param>
    /// <returns>如果字符串是有效的 IPv4 地址，则返回 true；否则返回 false。</returns>
    /// <example>
    /// <code>
    /// bool ok = "192.168.1.1".IsIpv4();   // true
    /// bool no = "999.1.1.1".IsIpv4();     // false
    /// bool no2 = "::1".IsIpv4();          // false (IPv6)
    /// bool no3 = "192.168.1".IsIpv4();    // false (简写形式不支持)
    /// </code>
    /// </example>
    public static bool IsIpv4(this string? input)
    {
        return input is not null && IsIpv4Core(input);
    }

    /// <summary>
    /// 判断指定字符串是否为有效的 IPv6 地址。
    /// </summary>
    /// <param name="input">要验证的字符串。</param>
    /// <returns>如果字符串是有效的 IPv6 地址，则返回 true；否则返回 false。</returns>
    /// <example>
    /// <code>
    /// bool ok1 = "::1".IsIpv6();                    // true
    /// bool ok2 = "2001:db8::1".IsIpv6();           // true
    /// bool no = "192.168.1.1".IsIpv6();            // false (IPv4)
    /// </code>
    /// </example>
    public static bool IsIpv6(this string? input)
    {
        return System.Net.IPAddress.TryParse(input, out var ip)
            && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// Determines whether the specified string is a valid domain name.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid domain name; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok = "example.com".IsDomainName();     // true
    /// bool ok2 = "sub.example.co.uk".IsDomainName(); // true
    /// bool no = "-bad..domain".IsDomainName();    // false
    /// </code>
    /// </example>
    public static bool IsDomainName(this string str)
    {
        return str is not null && GetDomainRegex().IsMatch(str);
    }

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid email address; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok = "user@example.com".IsEmail();  // true
    /// bool no = "user@@example.com".IsEmail(); // false
    /// </code>
    /// </example>
    public static bool IsEmail(this string? input)
    {
        return input is not null && GetEmailRegex().IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string contains multiple valid email addresses.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains multiple valid email addresses; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok = "a@ex.com; b@ex.com".IsMultipleEmail(); // true
    /// bool no = "a@ex.com; bad@".IsMultipleEmail();     // false
    /// </code>
    /// </example>
    public static bool IsMultipleEmail(this string input)
    {
        return input is not null && GetMultipleMailRegex().IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string contains only a combination of English letters and numbers.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="minLength">The minimum length of the string.</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <returns>True if the string contains only a combination of English letters and numbers; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok1 = "abc123".IsCombinationOfEnglishNumber();        // true
    /// bool ok2 = "a1".IsCombinationOfEnglishNumber(2, null);     // true (&gt;=2)
    /// bool no1 = "abcdef".IsCombinationOfEnglishNumber();        // false (no digit)
    /// bool no2 = "123456".IsCombinationOfEnglishNumber();        // false (no letter)
    /// </code>
    /// </example>
    public static bool IsCombinationOfEnglishNumber(this string input, int? minLength = null, int? maxLength = null)
    {
        return IsCombination(input, minLength, maxLength, allowSymbols: false);
    }

    /// <summary>
    /// Determines whether the specified string contains only a combination of English letters, numbers, and special characters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="minLength">The minimum length of the string.</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <returns>True if the string contains only a combination of English letters, numbers, and special characters; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// bool ok = "Ab1!".IsCombinationOfEnglishNumberSymbol(3, 10); // true
    /// bool no = "Ab1".IsCombinationOfEnglishNumberSymbol();        // false (no symbol)
    /// </code>
    /// </example>
    public static bool IsCombinationOfEnglishNumberSymbol(this string input, int? minLength = null,
        int? maxLength = null)
    {
        return IsCombination(input, minLength, maxLength, allowSymbols: true);
    }

    private static bool IsCombination(string? input, int? minLength, int? maxLength, bool allowSymbols)
    {
        if (input is null
            || input.Length == 0
            || minLength is < 0
            || maxLength is < 0
            || (minLength is not null && maxLength is not null && minLength > maxLength)
            || (minLength is not null && input.Length < minLength)
            || (maxLength is not null && input.Length > maxLength))
        {
            return false;
        }

        var hasAsciiLetter = false;
        var hasDigit = false;
        var hasSymbol = false;

        foreach (var character in input)
        {
            if (IsAsciiLetter(character))
            {
                hasAsciiLetter = true;
                continue;
            }

            if (character >= '0' && character <= '9')
            {
                hasDigit = true;
                continue;
            }

            if (!allowSymbols || character == '\n')
            {
                return false;
            }

            hasSymbol = true;
        }

        return hasAsciiLetter && hasDigit && (!allowSymbols || hasSymbol);
    }
}
