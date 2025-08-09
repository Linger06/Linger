using System.Globalization;

namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{
    public static IEnumerable<object[]> Data2 = new List<object[]>
    {
        new object[] { "A,", "A" },
        new object[] { ",A,", "A" },
        new object[] { ",A", "A" },
        new object[] { ",A,BC", "A,BC" },
        new object[] { ",A,BC,", "A,BC" },
        new object[] { "A,B,C,", "A,B,C" }
    };

    public static IEnumerable<object[]> Data = new List<object[]>
    {
        new object[] { "+2.36244E-13", 0.000000000000236244 },
        new object[] { "2.36244E-13", 0.000000000000236244 },
        new object[] { "-2.36244E-13", -0.000000000000236244 }
    };

    public static IEnumerable<object[]> BracketsData = new List<object[]>
    {
        new object[] { "（Hello）", "" },
        new object[] { "(Hello)", "" },
        new object[] { "Hello(World)", "Hello" },
        new object[] { "(World)Hello", "Hello" },
        new object[] { "Hello()World", "HelloWorld" }
    };

    public static IEnumerable<object[]> Substring2Data = new List<object[]>
    {
        new object[] { "1234567890", 2, "12" },
        new object[] { "1234567890", 0, "" },
        new object[] { "1234567890", 10, "1234567890" },
        new object[] { "1234567890", 11, "1234567890" }
    };

    public static IEnumerable<object[]> Substring3Data = new List<object[]>
    {
        new object[] { "1234567890", 2, "90" },
        new object[] { "1234567890", 0, "" },
        new object[] { "1234567890", 10, "1234567890" },
        new object[] { "1234567890", 11, "1234567890" }
    };

    public static IEnumerable<object[]> IsNumberData = new List<object[]>
    {
        new object[] { "1234567890", 10, 0, true },
        new object[] { "1234567890", 9, 0, false },
        new object[] { "123456.7890", 6, 4, true },
        new object[] { "123456.7890", 6, 3, false },
        new object[] { "1234.567890", 4, 5, false },
        new object[] { "1234.567890", 4, 6, true }
    };

    [Theory]
    [InlineData("1")]
    [InlineData("1 ")]
    [InlineData(" 1")]
    [InlineData("1.0")]
    [InlineData("1.0 ")]
    [InlineData("1.1")]
    [InlineData("0.1")]
    public void IsNumber(string value)
    {
        var result = value.IsDecimal();
        Assert.True(result);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("1A")]
    [InlineData(" ")]
    public void IsNumber2(string value)
    {
        var result = value.IsDecimal();
        Assert.False(result);
    }

    [Fact]
    public void DelLastComma()
    {
        // Type
        var @this = "A,";

        // Examples
        var result = @this.RemoveLastComma();

        // Unit Test
        Assert.Equal("A", result);
    }

    [Fact]
    public void DelLastNewLine()
    {
        var @this = "A";
        var result = @this.RemoveLastNewLine();
        Assert.Equal("A", result);

        @this = "A\r\n";
        result = @this.RemoveLastNewLine();
        Assert.Equal("A", result);

        @this = "\r\nA";
        result = @this.RemoveLastNewLine();
        Assert.Equal("\r\nA", result);

        @this = "\r\nA\r\n";
        result = @this.RemoveLastNewLine();
        Assert.Equal("\r\nA", result);

        @this = """
                A

                """;
        result = @this.RemoveLastNewLine();
        Assert.Equal("A", result);

        @this = " ";
        result = @this.RemoveLastNewLine();
        Assert.Equal(" ", result);

        @this = """

                A

                """;
        result = @this.RemoveLastNewLine();
        Assert.Equal("""

                     A
                     """, result);
    }

    [Theory]
    [InlineData("1/18/2016")]
    public void IsDateTime(string value)
    {
        var result = value.IsDateTime("M/d/yyyy");
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(Data2))]
    public void DelPrefixAndSuffix(string value1, string value2)
    {
        var result = value1.RemovePrefixAndSuffix(',');
        Assert.Equal(value2, result);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ToDecimal2(string value, decimal value2)
    {
        var result = value.ToDecimalForScientificNotation();
        var result2 = value2.ToString(CultureInfo.CurrentCulture);
        Assert.Equal(result.ToString(CultureInfo.CurrentCulture), result2);
    }

    [Theory]
    [MemberData(nameof(Substring2Data))]
    public void Substring2Test(string value, int value2, string value3)
    {
        Assert.Equal(value.Substring2(value2), value3);
    }

    [Theory]
    [MemberData(nameof(Substring3Data))]
    public void Substring3Test(string value, int value2, string value3)
    {
        Assert.Equal(value.Substring3(value2), value3);
    }

    [Theory]
    [MemberData(nameof(IsNumberData))]
    public void IsNumberTest(string value, int value2, int value3, bool value4)
    {
        Assert.Equal(value.IsNumber(value2, value3), value4);
    }

    [Fact]
    public void IsEmptyTest()
    {
        // Type
        var thisValue = "Fizz";
        string? thisNull = null;

        // Examples
        var value1 = thisValue.IsNullOrEmpty(); // return false;
        var value2 = thisNull.IsNullOrEmpty(); // return true;

        // Unit Test
        Assert.False(value1);
        Assert.True(value2);
    }

    [Theory]
    [InlineData(" ABCDEF", " ", "ABCDEF")]
    [InlineData("ABCDEF ", " ", "ABCDEF")]
    [InlineData(" ABCDEF ", " ", "ABCDEF")]
    [InlineData("%ABCDEF", "%", "ABCDEF")]
    [InlineData("123ABCDEF", "123", "ABCDEF")]
    [InlineData("ABCDEF456", "456", "ABCDEF")]
    public void DelPrefixAndSuffixTest(string value, string value2, string value3)
    {
        var result = value.RemovePrefixAndSuffix(value2);
        Assert.Equal(value3, result);
    }

    [Fact]
    public void IsNull_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNull_NonNullString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_EmptyString_ReturnsTrue()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_NonEmptyString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrEmpty_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_EmptyString_ReturnsTrue()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_NonEmptyString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_WhiteSpaceString_ReturnsTrue()
    {
        // Arrange
        var value = "   ";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_NonWhiteSpaceString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNull_NonNullString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotNull_NullString_ReturnsFalse()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNotNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotEmpty_NonEmptyString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotEmpty_EmptyString_ReturnsFalse()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNotEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_NonNullNonEmptyString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_NullString_ReturnsFalse()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_EmptyString_ReturnsFalse()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.False(result);
    }

    // New naming equivalents
    [Fact]
    public void IsNotNullOrEmpty_NewName_WorksSameAsOld()
    {
        var value = "abc";
        Assert.True(value.IsNotNullOrEmpty());
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_NewName_WorksSameAsOld()
    {
        var value = "abc";
        Assert.True(value.IsNotNullOrWhiteSpace());
    }

    [Fact]
    public void IsNotNullOrEmpty_Null_ReturnsFalse()
    {
        string? v = null;
        Assert.False(v.IsNotNullOrEmpty());
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_Whitespace_ReturnsFalse()
    {
        var v = "   ";
        Assert.False(v.IsNotNullOrWhiteSpace());
    }

    [Fact]
    public void IsInt16_ValidInt16String_ReturnsTrue()
    {
        // Arrange
        var value = "12345";

        // Act
        var result = value.IsInt16();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt16_InvalidInt16String_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt16();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInt_ValidIntString_ReturnsTrue()
    {
        // Arrange
        var value = "1234567890";

        // Act
        var result = value.IsInt();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt_InvalidIntString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInt64_ValidInt64String_ReturnsTrue()
    {
        // Arrange
        var value = "1234567890123456789";

        // Act
        var result = value.IsInt64();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt64_InvalidInt64String_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt64();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDecimal_ValidDecimalString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsDecimal();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDecimal_InvalidDecimalString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDecimal();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSingle_ValidSingleString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsSingle();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSingle_InvalidSingleString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsSingle();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDouble_ValidDoubleString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsDouble();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDouble_InvalidDoubleString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDouble();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_ValidDateTimeString_ReturnsTrue()
    {
        // Arrange
        var value = "2021-10-01";

        // Act
        var result = value.IsDateTime("yyyy-MM-dd");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDateTime_InvalidDateTimeString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDateTime();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("2023-04-15", "yyyy-MM-dd", true)]
    [InlineData("15/04/2023", "dd/MM/yyyy", true)]
    [InlineData("04/15/2023", "MM/dd/yyyy", true)]
    [InlineData("2023.04.15", "yyyy.MM.dd", true)]
    [InlineData("15-Apr-2023", "dd-MMM-yyyy", true)]
    [InlineData("Saturday, 15 April 2023", "dddd, dd MMMM yyyy", true)]
    [InlineData("2023-04-15 14:30:25", "yyyy-MM-dd HH:mm:ss", true)]
    [InlineData("2023-04-15T14:30:25", "yyyy-MM-ddTHH:mm:ss", true)]
    [InlineData("20230415", "yyyyMMdd", true)]
    [InlineData("2023-13-15", "yyyy-MM-dd", false)] // 无效月份
    [InlineData("2023-04-32", "yyyy-MM-dd", false)] // 无效日期
    [InlineData("2023/04/15", "yyyy-MM-dd", false)] // 格式不匹配
    [InlineData("not-a-date", "yyyy-MM-dd", false)] // 完全无效
    [InlineData("", "yyyy-MM-dd", false)]           // 空字符串
    [InlineData(null, "yyyy-MM-dd", false)]         // null
    public void IsDateTime_WithFormat_ShouldReturnExpectedResult(string? input, string format, bool expected)
    {
        var result = input.IsDateTime(format);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsDateTime_WithFormatArray_ValidDateMultipleFormats_ReturnsTrue()
    {
        // Arrange
        var dateStr = "2023-04-15";
        var formats = new[] { "yyyy/MM/dd", "yyyy-MM-dd", "yyyyMMdd" };

        // Act
        var result = dateStr.IsDateTime(formats);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDateTime_WithFormatArray_ValidDateNoMatchingFormat_ReturnsFalse()
    {
        // Arrange
        var dateStr = "2023-04-15";
        var formats = new[] { "yyyy/MM/dd", "dd/MM/yyyy", "MM/dd/yyyy" };

        // Act
        var result = dateStr.IsDateTime(formats);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_WithFormatArray_InvalidDate_ReturnsFalse()
    {
        // Arrange
        var dateStr = "2023-13-45"; // 无效月份和日期
        var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd" };

        // Act
        var result = dateStr.IsDateTime(formats);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_WithFormatArray_NullInput_ReturnsFalse()
    {
        // Arrange
        string? dateStr = null;
        var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd" };

        // Act
        var result = dateStr.IsDateTime(formats);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_WithFormatArray_EmptyInput_ReturnsFalse()
    {
        // Arrange
        var dateStr = string.Empty;
        var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd" };

        // Act
        var result = dateStr.IsDateTime(formats);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_DefaultOverload_InvalidDateString_ReturnsFalse()
    {
        // Arrange
        var input = "not-a-date";

        // Act
        var result = input.IsDateTime();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", true)]
    [InlineData("abc", false)]
    public void IsBoolean_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsBoolean();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("d3c4a2b1-4f5e-6d7c-8b9a-0f1e2d3c4b5a", true)]
    [InlineData("abc", false)]
    public void IsGuid_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsGuid();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("d3c4a2b14f5e6d7c8b9a0f1e2d3c4b5a", "N", true)]
    [InlineData("abc", "N", false)]
    public void IsGuid_WithFormat_ShouldReturnExpectedResult(string value, string format, bool expected)
    {
        var result = value.IsGuid(format);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]        // null string should return false
    [InlineData("", true)]           // empty string should return true
    [InlineData("   ", true)]        // whitespace string should return true
    [InlineData(" \t\n\r", true)]   // various whitespace characters should return true
    [InlineData("abc", false)]       // non-whitespace string should return false
    [InlineData(" abc ", false)]     // string with whitespace and non-whitespace should return false
    [InlineData("\u0020", true)]     // space character should return true
    [InlineData("\u0009", true)]     // tab character should return true
    public void IsWhiteSpace_ShouldReturnExpectedResult(string? input, bool expected)
    {
        // Act
        var result = input.IsWhiteSpace();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsWhiteSpace_WithAllWhitespaceCharacters_ReturnsTrue()
    {
        // Arrange
        var allWhitespace = new string(new[] { ' ', '\t', '\n', '\r', '\f', '\v' });

        // Act
        var result = allWhitespace.IsWhiteSpace();

        // Assert
        Assert.True(result);
    }

    // 正则表达式相关方法测试

    [Theory]
    [InlineData("abc", true)]
    [InlineData("ABC", true)]
    [InlineData("AbCdEf", true)]
    [InlineData("abc123", false)]
    [InlineData("abc_def", false)]
    [InlineData("abc-def", false)]
    [InlineData("abc def", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsEnglish_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsEnglish();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com", true)]
    [InlineData("https://example.com.net", true)]
    [InlineData("http://example.com/api", true)]
    [InlineData("http://example.com/src/", true)]
    [InlineData("http://example.com/src/test.html", true)]
    [InlineData("http://example.com/src/test.2.0.0.zip", true)]
    [InlineData("http://example.com/src/test.html?id=1", true)]
    [InlineData("hhhhhttps://example.com", false)]
    [InlineData("invalid_url", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsUrl_ShouldReturnExpectedResult(string? value, bool expected)
    {
        var result = value.IsUrl();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("test.user@example.co.uk", true)]
    [InlineData("test+label@example.com", true)]
    [InlineData("test@", false)]
    [InlineData("@example.com", false)]
    [InlineData("test@example", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsEmail_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsEmail();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("test@example.com;user@example.org", true)]
    [InlineData("test@example.com; user@example.org", true)]
    [InlineData("invalid@email", false)]
    [InlineData("test@example.com;invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsMultipleEmail_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsMultipleEmail();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("example.com", true)]
    [InlineData("subdomain.example.com", true)]
    [InlineData("sub-domain.example.co.uk", true)]
    [InlineData("example", false)]
    [InlineData("", false)]
    [InlineData("example..com", false)]
    [InlineData(null, false)]
    public void IsDomainName_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsDomainName();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("256.0.0.1", false)]
    [InlineData("192.168.1", false)]
    [InlineData("192.168.1.1.1", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsIpv4_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsIpv4();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1E+2", true)]
    [InlineData("1.23E-4", true)]
    [InlineData("1.23e+10", true)]
    [InlineData("1.23", false)]
    [InlineData("123", false)]
    [InlineData("E123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsScientificNotation_ShouldReturnExpectedResult(string? input, bool expected)
    {
        var result = input.IsScientificNotation();
        Assert.Equal(expected, result);
    }

    // 字符串处理方法测试

    [Theory]
    [InlineData("abc123!", true, null, null)]
    [InlineData("a1!", true, 3, null)]
    [InlineData("abc12345!", true, null, 9)]
    [InlineData("abc123!", true, 3, 7)]
    [InlineData("abcdef", false, null, null)]
    [InlineData("123456", false, null, null)]
    [InlineData("abc123", false, null, null)]
    public void IsCombinationOfEnglishNumberSymbol_ShouldReturnExpectedResult(string input, bool expected, int? minLength, int? maxLength)
    {
        var result = input.IsCombinationOfEnglishNumberSymbol(minLength, maxLength);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc123", true, null, null)]
    [InlineData("a1", true, 2, null)]
    [InlineData("abc12345", true, null, 8)]
    [InlineData("abc123", true, 3, 6)]
    [InlineData("abcdef", false, null, null)]
    [InlineData("123456", false, null, null)]
    [InlineData("a1", false, 3, null)]
    public void IsCombinationOfEnglishNumber_ShouldReturnExpectedResult(string input, bool expected, int? minLength, int? maxLength)
    {
        var result = input.IsCombinationOfEnglishNumber(minLength, maxLength);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", 10, null, "hello")]
    [InlineData("hello world", 5, null, "he...")]
    [InlineData("hello world", 5, "***", "he***")]
    [InlineData("hello world", 2, "***", "***")]
    [InlineData("hello", 0, null, "")]
    [InlineData("", 5, null, "")]
    [InlineData(null, 5, null, "")]
    public void Truncate_ShouldReturnExpectedResult(string? input, int maxLength, string? suffix, string expected)
    {
        string actualSuffix = suffix ?? "...";
        var result = input.Truncate(maxLength, actualSuffix);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("[hello]", '[', "hello]")]
    [InlineData("[hello", '[', "hello")]
    [InlineData("hello]", '[', "hello]")]
    [InlineData("hello", '[', "hello")]
    [InlineData("", '[', "")]
    [InlineData(null, '[', null)]
    public void DelPrefixAndSuffix_CharOverload_ShouldReturnExpectedResult(string? input, char value, string? expected)
    {
        var result = input.RemovePrefixAndSuffix(value);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<hello>", "<>", "<hello>")]
    [InlineData("<hello", "<>", "<hello")]
    [InlineData("hello>", "<>", "hello>")]
    [InlineData("hello", "<>", "hello")]
    [InlineData("", "<>", "")]
    [InlineData(null, "<>", null)]
    public void DelPrefixAndSuffix_StringOverload_ShouldReturnExpectedResult(string? input, string value, string? expected)
    {
        var result = input.RemovePrefixAndSuffix(value);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToMd5HashByte_ValidString_ReturnsValidHash()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = input.ToMd5HashByte();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(16, result.Length); // MD5 hash is always 16 bytes
    }

    [Fact]
    public void ToMd5HashByte_EmptyString_ReturnsEmptyArray()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.ToMd5HashByte();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToMd5HashCode_ValidString_ReturnsValidHashString()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = input.ToMd5HashCode();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(32, result.Length); // MD5 hash string is always 32 characters (16 bytes * 2 hex digits)
        Assert.True(result.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))); // Only uppercase hex characters
    }

    [Fact]
    public void ToMd5HashCode_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.ToMd5HashCode();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSha256HashByte_ValidString_ReturnsValidHash()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = input.ToSha256HashByte();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(32, result.Length); // SHA256 hash is always 32 bytes
    }

    [Fact]
    public void ToSha256HashByte_EmptyString_ReturnsEmptyArray()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.ToSha256HashByte();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToSha256HashCode_ValidString_ReturnsValidHashString()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = input.ToSha256HashCode();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(64, result.Length); // SHA256 hash string is always 64 characters (32 bytes * 2 hex digits)
        Assert.True(result.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))); // Only hex characters
    }

    [Fact]
    public void ToSha256HashCode_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.ToSha256HashCode();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    // Base64转换方法测试

    [Theory]
    [InlineData("SGVsbG8gV29ybGQ=", "Hello World")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void FromBase64ToString_ShouldReturnExpectedResult(string? input, string expected)
    {
        var result = input.FromBase64ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello World", "SGVsbG8gV29ybGQ=")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ToBase64String_ShouldReturnExpectedResult(string? input, string expected)
    {
        var result = input.ToBase64String();
        Assert.Equal(expected, result);
    }

    // 其他实用方法测试

    [Theory]
    [InlineData("test@example.com", "test")]
    [InlineData("user.name@domain.co.uk", "user.name")]
    [InlineData("invalid", "")]
    [InlineData("@invalid.com", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void GetEmailPrefix_ShouldReturnExpectedResult(string? input, string expected)
    {
        var result = input.GetEmailPrefix();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "id=123", "http://example.com?id=123")]
    [InlineData("http://example.com?param=value", "id=123", "http://example.com?param=value&id=123")]
    [InlineData("", "id=123", "?id=123")]
    public void AppendQuery_StringOverload_ShouldReturnExpectedResult(string self, string query, string expected)
    {
        var result = self.AppendQuery(query);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppendQuery_DictionaryOverload_ShouldAppendQueryParameters()
    {
        // Arrange
        var url = "http://example.com";
        var data = new Dictionary<string, string>
        {
            ["id"] = "123",
            ["name"] = "test"
        };

        // Act
        var result = url.AppendQuery(data);

        // Assert
        Assert.Contains("?id=123", result);
        Assert.Contains("name=test", result);
        Assert.StartsWith("http://example.com?", result);
    }

    [Fact]
    public void AppendQuery_KeyValuePairListOverload_ShouldAppendQueryParameters()
    {
        // Arrange
        var url = "http://example.com";
        var data = new List<KeyValuePair<string, string>>
        {
            new("id", "123"),
            new("name", "test")
        };

        // Act
        var result = url.AppendQuery(data);

        // Assert
        Assert.Contains("?id=123", result);
        Assert.Contains("name=test", result);
        Assert.StartsWith("http://example.com?", result);
    }

    [Theory]
    [InlineData("Hello\r\nWorld", "Hello\r\nWorld")]
    [InlineData("Hello\r\n", "Hello")]
    [InlineData("Hello\n", "Hello")]
    [InlineData("Hello", "Hello")]
    public void DelLastNewLine_ShouldReturnExpectedResult(string input, string expected)
    {
        var result = input.RemoveLastNewLine();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello\r\nWorld", "HelloWorld")]
    [InlineData("Hello\nWorld\r\n", "HelloWorld")]
    [InlineData("Hello\rWorld", "HelloWorld")]
    [InlineData("Hello", "Hello")]
    [InlineData(null, "")]
    public void DelAllNewLine_ShouldReturnExpectedResult(string? input, string expected)
    {
        var result = input.RemoveAllNewLine();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello,", "Hello")]
    [InlineData("Hello, World,", "Hello, World")]
    [InlineData("Hello", "Hello")]
    [InlineData(",", "")]
    [InlineData("", "")]
    public void DelLastComma_ShouldReturnExpectedResult(string input, string expected)
    {
        var result = input.RemoveLastComma();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello123", "123", "Hello")]
    [InlineData("123Hello123", "123", "123Hello")]
    [InlineData("Hello", "123", "Hello")]
    [InlineData("", "123", "")]
    [InlineData(null, "123", "")]
    public void DelLastChar_ShouldReturnExpectedResult(string? input, string character, string expected)
    {
        var result = input.RemoveLastChar(character);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/api", "/", "/api")]
    [InlineData("api", "/", "/api")]
    [InlineData(null, "/", "/")]
    [InlineData("/api", null, "/api")]
    [InlineData(null, null, "")]
    public void EnsureStartsWith_ShouldReturnExpectedResult(string? value, string? prefix, string expected)
    {
        var result = value.EnsureStartsWith(prefix);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("file.txt", ".txt", "file.txt")]
    [InlineData("file", ".txt", "file.txt")]
    [InlineData(null, ".txt", ".txt")]
    [InlineData("file.txt", null, "file.txt")]
    [InlineData(null, null, "")]
    public void EnsureEndsWith_ShouldReturnExpectedResult(string? value, string? suffix, string expected)
    {
        var result = value.EnsureEndsWith(suffix);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello World", 5, "Hello")]
    [InlineData("Hello", 10, "Hello")]
    [InlineData("", 5, "")]
    [InlineData("Hello", 0, "")]
    [InlineData(null, 5, "")]
    public void TruncateFromStart_ShouldReturnExpectedResult(string? input, int length, string expected)
    {
        var result = input.Take(length);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello World", 5, "World")]
    [InlineData("Hello", 10, "Hello")]
    [InlineData("", 5, "")]
    [InlineData("Hello", 0, "")]
    [InlineData(null, 5, "")]
    public void TakeLast_ShouldReturnExpectedResult(string? input, int length, string expected)
    {
        var result = input.TakeLast(length);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("123.45", false)]
    [InlineData("-123", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void IsInteger_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = input.IsInteger();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("-123", false)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void IsPositiveInteger_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = input.IsPositiveInteger();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true, 3, 0)]
    [InlineData("123.45", true, 5, 2)]
    [InlineData("123.456", false, 5, 2)]
    [InlineData("1234567", false, 5, 0)]
    public void IsNumber_ShouldReturnExpectedResult(string input, bool expected, int precision, int scale)
    {
        var result = input.IsNumber(precision, scale);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDecimalForScientificNotation_ValidInput_ReturnsDecimal()
    {
        // Arrange
        var input = "1.23E+2";

        // Act
        var result = input.ToDecimalForScientificNotation();

        // Assert
        Assert.Equal(123m, result);
    }

    [Fact]
    public void ToDecimalForScientificNotation_InvalidInput_ThrowsFormatException()
    {
        // Arrange
        var input = "123";

        // Act & Assert
        Assert.Throws<FormatException>(() => input.ToDecimalForScientificNotation());
    }
}