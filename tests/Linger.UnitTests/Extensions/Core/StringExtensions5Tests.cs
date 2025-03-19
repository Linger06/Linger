namespace Linger.UnitTests.Extensions.Core;

public class StringExtensions5Tests
{
    [Theory]
    [InlineData("test", 2, "st")]
    [InlineData("test", 10, "test")]
    public void Substring3_ShouldReturnExpectedResult(string value, int length, string expected)
    {
        var result = value.Substring3(length);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.23E3", 1230)]
    [InlineData("1.23E-3", 0.00123)]
    public void ToDecimal2_ShouldReturnExpectedResult(string value, decimal expected)
    {
        var result = value.ToDecimalForScientificNotation();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", 't', "es")]
    [InlineData("test", 'e', "test")]
    public void DelPrefixAndSuffix_Char_ShouldReturnExpectedResult(string value, char character, string expected)
    {
        var result = value.DelPrefixAndSuffix(character);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", "t", "es")]
    [InlineData("test", "e", "test")]
    public void DelPrefixAndSuffix_String_ShouldReturnExpectedResult(string value, string character, string expected)
    {
        var result = value.DelPrefixAndSuffix(character);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    public void IsEmail_ShouldReturnExpectedResult(string? value, bool expected)
    {
        var result = value.IsEmail();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("example.com", true)]
    [InlineData("invalid_domain", false)]
    public void IsDomainName_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsDomainName();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.0.1", true)]
    [InlineData("::1", true)]
    [InlineData("invalid_ip", false)]
    public void IsIpAddress_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsIpAddress();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.0.1", true)]
    [InlineData("invalid_ip", false)]
    public void IsIpv4_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsIpv4();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("::1", true)]
    [InlineData("invalid_ip", false)]
    public void IsIpv6_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsIpv6();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", true)]
    [InlineData("invalid_url", false)]
    public void IsUrl_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsUrl();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("13800138000", true)]
    [InlineData("invalid_phone", false)]
    public void IsPhoneNumber_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsPhoneNumber();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", true)]
    [InlineData("test123", false)]
    public void IsEnglish_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsEnglish();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test123", null, null, true)]
    [InlineData("test", null, null, false)]
    [InlineData("test123", 4, null, true)]
    [InlineData("test123", null, 7, true)]
    [InlineData("test123", 4, 7, true)]
    [InlineData("test123", 5, 7, true)]
    public void IsCombinationOfEnglishNumber_ShouldReturnExpectedResult(string value, int? minLength, int? maxLength, bool expected)
    {
        var result = value.IsCombinationOfEnglishNumber(minLength, maxLength);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test123!", null, null, true)]
    [InlineData("test123", null, null, false)]
    [InlineData("test123!", 4, null, true)]
    [InlineData("test123!", null, 8, true)]
    [InlineData("test123!", 4, 8, true)]
    [InlineData("test123!", 5, 7, false)]
    public void IsCombinationOfEnglishNumberSymbol_ShouldReturnExpectedResult(string value, int? minLength, int? maxLength, bool expected)
    {
        var result = value.IsCombinationOfEnglishNumberSymbol(minLength, maxLength);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Substring3_ReturnsFullString_WhenLengthIsGreaterThanStringLength()
    {
        var input = "hello";
        var result = input.Substring3(10);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Substring3_ReturnsLastNCharacters_WhenLengthIsLessThanStringLength()
    {
        var input = "hello";
        var result = input.Substring3(3);
        Assert.Equal("llo", result);
    }

    [Fact]
    public void ToDecimal2_ReturnsDecimalValue_WhenInputIsValidScientificNotation()
    {
        var input = "1.23e4";
        var result = input.ToDecimalForScientificNotation();
        Assert.Equal(12300m, result);
    }

    [Fact]
    public void ToDecimal2_ThrowsFormatException_WhenInputIsInvalid()
    {
        var input = "invalid";
        Assert.Throws<FormatException>(() => input.ToDecimalForScientificNotation());
    }

    [Fact]
    public void DelPrefixAndSuffix_RemovesSingleCharacterPrefixAndSuffix()
    {
        var input = "*hello*";
        var result = input.DelPrefixAndSuffix('*');
        Assert.Equal("hello", result);
    }

    [Fact]
    public void DelPrefixAndSuffix_RemovesMultipleCharacterPrefixAndSuffix()
    {
        var input = "##hello##";
        var result = input.DelPrefixAndSuffix("##");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void IsEmail_ReturnsTrue_WhenInputIsValidEmail()
    {
        var input = "test@example.com";
        var result = input.IsEmail();
        Assert.True(result);
    }

    [Fact]
    public void IsEmail_ReturnsFalse_WhenInputIsInvalidEmail()
    {
        var input = "invalid-email";
        var result = input.IsEmail();
        Assert.False(result);
    }

    [Fact]
    public void IsDomainName_ReturnsTrue_WhenInputIsValidDomain()
    {
        var input = "example.com";
        var result = input.IsDomainName();
        Assert.True(result);
    }

    [Fact]
    public void IsDomainName_ReturnsFalse_WhenInputIsInvalidDomain()
    {
        var input = "invalid_domain";
        var result = input.IsDomainName();
        Assert.False(result);
    }

    [Fact]
    public void IsIpAddress_ReturnsTrue_WhenInputIsValidIpAddress()
    {
        var input = "192.168.1.1";
        var result = input.IsIpAddress();
        Assert.True(result);
    }

    [Fact]
    public void IsIpAddress_ReturnsFalse_WhenInputIsInvalidIpAddress()
    {
        var input = "999.999.999.999";
        var result = input.IsIpAddress();
        Assert.False(result);
    }

    [Fact]
    public void IsIpv4_ReturnsTrue_WhenInputIsValidIpv4()
    {
        var input = "192.168.1.1";
        var result = input.IsIpv4();
        Assert.True(result);
    }

    [Fact]
    public void IsIpv4_ReturnsFalse_WhenInputIsInvalidIpv4()
    {
        var input = "invalid-ip";
        var result = input.IsIpv4();
        Assert.False(result);
    }

    [Fact]
    public void IsIpv6_ReturnsTrue_WhenInputIsValidIpv6()
    {
        var input = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";
        var result = input.IsIpv6();
        Assert.True(result);
    }

    [Fact]
    public void IsIpv6_ReturnsFalse_WhenInputIsInvalidIpv6()
    {
        var input = "invalid-ip";
        var result = input.IsIpv6();
        Assert.False(result);
    }

    [Fact]
    public void IsUrl_ReturnsTrue_WhenInputIsValidUrl()
    {
        var input = "https://www.example.com";
        var result = input.IsUrl();
        Assert.True(result);
    }

    [Fact]
    public void IsUrl_ReturnsFalse_WhenInputIsInvalidUrl()
    {
        var input = "invalid-url";
        var result = input.IsUrl();
        Assert.False(result);
    }

    [Fact]
    public void IsPhoneNumber_ReturnsTrue_WhenInputIsValidChinesePhoneNumber()
    {
        var input = "13800138000";
        var result = input.IsPhoneNumber();
        Assert.True(result);
    }

    [Fact]
    public void IsPhoneNumber_ReturnsFalse_WhenInputIsInvalidChinesePhoneNumber()
    {
        var input = "123456";
        var result = input.IsPhoneNumber();
        Assert.False(result);
    }

    [Fact]
    public void IsEnglish_ReturnsTrue_WhenInputIsOnlyEnglishLetters()
    {
        var input = "hello";
        var result = input.IsEnglish();
        Assert.True(result);
    }

    [Fact]
    public void IsEnglish_ReturnsFalse_WhenInputContainsNonEnglishLetters()
    {
        var input = "hello123";
        var result = input.IsEnglish();
        Assert.False(result);
    }

    [Fact]
    public void IsCombinationOfEnglishNumber_ReturnsTrue_WhenInputIsValid()
    {
        var input = "hello123";
        var result = input.IsCombinationOfEnglishNumber();
        Assert.True(result);
    }

    [Fact]
    public void IsCombinationOfEnglishNumber_ReturnsFalse_WhenInputContainsSpecialCharacters()
    {
        var input = "hello123!";
        var result = input.IsCombinationOfEnglishNumber();
        Assert.False(result);
    }

    [Fact]
    public void IsCombinationOfEnglishNumberSymbol_ReturnsTrue_WhenInputIsValid()
    {
        var input = "hello123!";
        var result = input.IsCombinationOfEnglishNumberSymbol();
        Assert.True(result);
    }

    [Fact]
    public void IsCombinationOfEnglishNumberSymbol_ReturnsFalse_WhenInputContainsInvalidCharacters()
    {
        var input = "hello123!@#";
        var result = input.IsCombinationOfEnglishNumberSymbol();
        Assert.True(result);
    }
}