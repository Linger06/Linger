using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    #region Regex Constants
    const string Ipv4RegexPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
    const string DomainRegexPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$";
    const string UrlRegexPattern = @"^https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]";
    const string EnglishRegexPattern = "^[A-Za-z]+$";
    const string EmailRegexPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    const string MultipleMailRegexPattern = @"^((?:(?:[a-zA-Z0-9_\-\.]+)@(?:(?:\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(?:(?:[a-zA-Z0-9\-]+\.)+))(?:[a-zA-Z]{2,4}|[0-9]{1,3})(?:\]?)(?:\s*;\s*|\s*$))+)$";
    #endregion

#if NET8_0_OR_GREATER
    [GeneratedRegex(EnglishRegexPattern)]
    private static partial Regex EnglishRegex();

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
        if (input == null)
            return false;

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
}
