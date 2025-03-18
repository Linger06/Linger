using System.Globalization;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="string"/> extensions.
/// </summary>
public static partial class StringExtensions
{
    const string Ipv4RegexPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
    const string Ipv6RegexPattern = @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$";
    const string DomainRegexPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$";
    const string UrlRegexPattern = @"^[a-zA-z]+://[^\s]*$";
    const string PhoneNumberRegexPattern = @"^(13[0-9]|14[5|7]|15[0|1|2|3|4|5|6|7|8|9]|18[0|1|2|3|5|6|7|8|9])\d{8}$";
    const string EnglishRegexPattern = "^[A-Za-z]+$";
    const string EmailRegexPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    const string MultipleMailRegexPattern = @"^((?:(?:[a-zA-Z0-9_\-\.]+)@(?:(?:\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(?:(?:[a-zA-Z0-9\-]+\.)+))(?:[a-zA-Z]{2,4}|[0-9]{1,3})(?:\]?)(?:\s*;\s*|\s*$))*)$";
#if NET8_0_OR_GREATER

    [GeneratedRegex(PhoneNumberRegexPattern)]
    private static partial Regex PhoneNumberRegex();

    /// <summary>
    /// Determines whether the specified string is a valid phone number (China mainland).
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid phone number; otherwise, false.</returns>
    public static bool IsPhoneNumber(this string input)
    {
        return PhoneNumberRegex().IsMatch(input);
    }

    [GeneratedRegex(EnglishRegexPattern)]
    private static partial Regex EnglishRegex();

    /// <summary>
    /// Determines whether the specified string contains only English letters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
    public static bool IsEnglish(this string input)
    {
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
        return Ipv4Regex().IsMatch(input);
    }

    [GeneratedRegex(Ipv6RegexPattern)]
    private static partial Regex Ipv6Regex();

    /// <summary>
    /// Determines whether the specified string is a valid IPv6 address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid IPv6 address; otherwise, false.</returns>
    public static bool IsIpv6(this string input)
    {
        return Ipv6Regex().IsMatch(input);
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
        return MultipleMailRegex().IsMatch(input);
    }

    [GeneratedRegex("[+-]?\\d+(\\.\\d+)?[eE][+-]?\\d+")]
    private static partial Regex ScientificNotationRegex();

    // 判断是否是科学计数法
    public static bool IsScientificNotation(this string input)
    {
        return ScientificNotationRegex().IsMatch(input);
    }

#else
    private static readonly Regex s_ipv4Regex = new(Ipv4RegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_ipv6Regex = new(Ipv6RegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_domainRegex = new(DomainRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_urlRegex = new(UrlRegexPattern, RegexOptions.Compiled);

    private static readonly Regex s_phoneNumberRegex = new(PhoneNumberRegexPattern, RegexOptions.Compiled);

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
        return s_multipleMailRegex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid domain name.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid domain name; otherwise, false.</returns>
    public static bool IsDomainName(this string str)
    {
        return s_domainRegex.IsMatch(str);
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv4 address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid IPv4 address; otherwise, false.</returns>
    public static bool IsIpv4(this string input)
    {
        return s_ipv4Regex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv6 address.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid IPv6 address; otherwise, false.</returns>
    public static bool IsIpv6(this string input)
    {
        return s_ipv6Regex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    public static bool IsUrl(this string input)
    {
        return s_urlRegex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string is a valid phone number (China mainland).
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid phone number; otherwise, false.</returns>
    public static bool IsPhoneNumber(this string input)
    {
        return s_phoneNumberRegex.IsMatch(input);
    }

    /// <summary>
    /// Determines whether the specified string contains only English letters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string contains only English letters; otherwise, false.</returns>
    public static bool IsEnglish(this string input)
    {
        return s_englishRegex.IsMatch(input);
    }

    // 判断是否是科学计数法
    public static bool IsScientificNotation(this string input)
    {
        return Regex.IsMatch(input, "[+-]?\\d+(\\.\\d+)?[eE][+-]?\\d+");
    }

#endif

    /// <summary>
    /// Determines whether the specified string is a valid IP address.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid IP address; otherwise, false.</returns>
    public static bool IsIpAddress(this string str)
    {
        return str.IsIpv4() || str.IsIpv6();
    }

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
}