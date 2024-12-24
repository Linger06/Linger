using System.Globalization;

namespace Linger.UnitTests.Extensions.Core;

public class StringExtensionsTest
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
        var result = @this.DelLastComma();

        // Unit Test
        Assert.Equal("A", result);
    }

    [Fact]
    public void DelLastNewLine()
    {
        var @this = "A";
        var result = @this.DelLastNewLine();
        Assert.Equal("A", result);

        @this = "A\r\n";
        result = @this.DelLastNewLine();
        Assert.Equal("A", result);

        @this = "\r\nA";
        result = @this.DelLastNewLine();
        Assert.Equal("\r\nA", result);

        @this = "\r\nA\r\n";
        result = @this.DelLastNewLine();
        Assert.Equal("\r\nA", result);

        @this = """
                A

                """;
        result = @this.DelLastNewLine();
        Assert.Equal("A", result);

        @this = " ";
        result = @this.DelLastNewLine();
        Assert.Equal(" ", result);

        @this = """

                A

                """;
        result = @this.DelLastNewLine();
        Assert.Equal("""

                     A
                     """, result);
    }

    [Theory]
    [InlineData("1/18/2016")]
    public void IsDateTime(string value)
    {
        var result = value.IsDateTime();
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(Data2))]
    public void DelPrefixAndSuffix(string value1, string value2)
    {
        var result = value1.DelPrefixAndSuffix(',');
        Assert.Equal(value2, result);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ToDecimal2(string value, decimal value2)
    {
        var result = value.ToDecimal2();
        var result2 = value2.ToString(CultureInfo.CurrentCulture);
        Assert.Equal(result.ToString(CultureInfo.CurrentCulture), result2);
    }

    [Theory]
    [MemberData(nameof(BracketsData))]
    public void DeleteBracketsTest(string value, string value2)
    {
        Assert.Equal(value.DeleteBrackets(), value2);
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
        var result = value.DelPrefixAndSuffix(value2);
        Assert.Equal(value3, result);
    }
}