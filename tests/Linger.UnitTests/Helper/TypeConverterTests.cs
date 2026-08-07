using System.Globalization;
using Linger.Helper;

namespace Linger.UnitTests.Helper;

public class TypeConverterTests
{
    [Fact]
    public void TryConvert_NullValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert(null, typeof(int?), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_DBNullValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert(DBNull.Value, typeof(string), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_NullTargetType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TypeConverter.TryConvert("test", null!, out _));
    }

    [Fact]
    public void TryConvert_ExactType_ReturnsSameValue()
    {
        var value = 42;

        var success = TypeConverter.TryConvert(value, typeof(int), out var result);

        Assert.True(success);
        Assert.Equal(value, result);
    }

    [Fact]
    public void TryConvert_AssignableTarget_ReturnsOriginalValue()
    {
        object value = new Uri("https://example.com");

        var success = TypeConverter.TryConvert(value, typeof(object), out var result);

        Assert.True(success);
        Assert.Same(value, result);
    }

    [Fact]
    public void TryConvert_StringToNumericTypes_ReturnsConvertedValues()
    {
        AssertConversion("123", 123);
        AssertConversion("9876543210", 9876543210L);
        AssertConversion("123.45", 123.45m);
        AssertConversion("3.14159", 3.14159d);
        AssertConversion("2.5", 2.5f);
    }

    [Fact]
    public void TryConvert_StringToUnsignedTypes_ReturnsConvertedValues()
    {
        AssertConversion("255", (byte)255);
        AssertConversion("-50", (sbyte)-50);
        AssertConversion("100", (ushort)100);
        AssertConversion("200", (uint)200);
        AssertConversion("300", (ulong)300);
    }

    [Fact]
    public void TryConvert_StringToBoolean_ReturnsConvertedValue()
    {
        AssertConversion("true", true);
        AssertConversion("off", false);
    }

    [Fact]
    public void TryConvert_StringToDateTime_ReturnsConvertedValue()
    {
        AssertConversion("2024-01-15", new DateTime(2024, 1, 15));
    }

    [Fact]
    public void TryConvert_ZhCnDateTimeString_ReturnsFalse()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");

            var success = TypeConverter.TryConvert("2024/1/15 下午 3:04:05", typeof(DateTime), out var result);

            Assert.False(success);
            Assert.Null(result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void TryConvert_OleAutomationDateToDateTime_ReturnsConvertedValue()
    {
        var value = new DateTime(2024, 1, 15).ToOADate();

        var success = TypeConverter.TryConvert(value, typeof(DateTime), out var result);

        Assert.True(success);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void TryConvert_InvalidOleAutomationDate_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert(double.MaxValue, typeof(DateTime), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_DateTimeOffsetToDateTime_UsesUtcValue()
    {
        var value = new DateTimeOffset(2024, 1, 15, 12, 30, 45, TimeSpan.FromHours(8));

        var success = TypeConverter.TryConvert(value, typeof(DateTime), out var result);

        Assert.True(success);
        var converted = Assert.IsType<DateTime>(result);
        Assert.Equal(value.UtcDateTime, converted);
        Assert.Equal(DateTimeKind.Utc, converted.Kind);
    }

    [Fact]
    public void TryConvert_StringToGuid_ReturnsConvertedValue()
    {
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        AssertConversion(expected.ToString(), expected);
    }

    [Fact]
    public void TryConvert_StringAndNumberToEnum_ReturnsConvertedValues()
    {
        AssertConversion("Friday", DayOfWeek.Friday);
        AssertConversion(5, DayOfWeek.Friday);
    }

    [Fact]
    public void TryConvert_StringToNullableTypes_ReturnsConvertedValues()
    {
        AssertConversion("123", (int?)123);
        AssertConversion("Friday", (DayOfWeek?)DayOfWeek.Friday);
    }

    [Fact]
    public void TryConvert_StringToTimeSpan_ReturnsConvertedValue()
    {
        AssertConversion("01:30:00", TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void TryConvert_InvalidNumericValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert("not a number", typeof(int), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_InvalidBooleanValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert(2, typeof(bool), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_UndefinedEnumValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert("999", typeof(DayOfWeek), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_InvalidTimeSpan_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert("invalid", typeof(TimeSpan), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_UnsupportedType_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert("https://example.com", typeof(Uri), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_CharTarget_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert("A", typeof(char), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_ArbitraryObjectToString_ReturnsFalse()
    {
        var success = TypeConverter.TryConvert(new Uri("https://example.com"), typeof(string), out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    private static void AssertConversion<T>(object value, T expected)
    {
        var success = TypeConverter.TryConvert(value, typeof(T), out var result);

        Assert.True(success);
        Assert.Equal(expected, result);
    }
}
