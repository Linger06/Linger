namespace Linger.UnitTests.Helper;

using Linger.Helper;

public class TypeConverterTests
{
    [Fact]
    public void ConvertTo_WithNullValue_ReturnsNull()
    {
        var result = TypeConverter.ConvertTo(null, typeof(int?));
        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_WithDBNullValue_ReturnsNull()
    {
        var result = TypeConverter.ConvertTo(DBNull.Value, typeof(string));
        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_WithExactTypeMatch_ReturnsSameValue()
    {
        var original = 42;
        var result = TypeConverter.ConvertTo(original, typeof(int));
        Assert.Equal(original, result);
    }

    [Fact]
    public void ConvertTo_StringToInt_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("123", typeof(int));
        Assert.Equal(123, result);
    }

    [Fact]
    public void ConvertTo_StringToNullableInt_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("456", typeof(int?));
        Assert.Equal(456, result);
    }

    [Fact]
    public void ConvertTo_StringToLong_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("9876543210", typeof(long));
        Assert.Equal(9876543210L, result);
    }

    [Fact]
    public void ConvertTo_StringToDecimal_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("123.45", typeof(decimal));
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ConvertTo_StringToDouble_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("3.14159", typeof(double));
        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void ConvertTo_StringToFloat_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("2.5", typeof(float));
        Assert.Equal(2.5f, result);
    }

    [Fact]
    public void ConvertTo_StringToBool_ReturnsConvertedValue()
    {
        Assert.Equal(true, TypeConverter.ConvertTo("true", typeof(bool)));
        Assert.Equal(false, TypeConverter.ConvertTo("false", typeof(bool)));
    }

    [Fact]
    public void ConvertTo_StringToDateTime_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("2024-01-15", typeof(DateTime));
        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void ConvertTo_DoubleToDateTime_ConvertsOADate()
    {
        // 45306.0 is the OLE Automation date for 2024-01-15
        var oaDate = new DateTime(2024, 1, 15).ToOADate();
        var result = TypeConverter.ConvertTo(oaDate, typeof(DateTime));
        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void ConvertTo_StringToGuid_ReturnsConvertedValue()
    {
        var guidString = "12345678-1234-1234-1234-123456789abc";
        var result = TypeConverter.ConvertTo(guidString, typeof(Guid));
        Assert.Equal(Guid.Parse(guidString), result);
    }

    [Fact]
    public void ConvertTo_StringToEnum_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("Friday", typeof(DayOfWeek));
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertTo_StringToEnumCaseInsensitive_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo("friday", typeof(DayOfWeek));
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertTo_IntToEnum_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo(5, typeof(DayOfWeek));
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertTo_IntToShort_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo(100, typeof(short));
        Assert.Equal((short)100, result);
    }

    [Fact]
    public void ConvertTo_IntToByte_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertTo(255, typeof(byte));
        Assert.Equal((byte)255, result);
    }

    [Fact]
    public void TryConvertTo_WithValidValue_ReturnsTrueAndConvertedValue()
    {
        var success = TypeConverter.TryConvertTo("123", typeof(int), out var result);
        Assert.True(success);
        Assert.Equal(123, result);
    }

    [Fact]
    public void TryConvertTo_WithInvalidValue_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a number", typeof(int), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_WithNullValue_ReturnsTrueWithNull()
    {
        var success = TypeConverter.TryConvertTo(null, typeof(int), out var result);
        Assert.True(success);
        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_WithNullTargetType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TypeConverter.ConvertTo("test", null!));
    }

    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("456", typeof(long), 456L)]
    [InlineData("true", typeof(bool), true)]
    [InlineData("test", typeof(string), "test")]
    public void ConvertTo_WithVariousTypes_ReturnsCorrectValue(object input, Type targetType, object expected)
    {
        var result = TypeConverter.ConvertTo(input, targetType);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertTo_StringToDecimalTheory_ReturnsCorrectValue()
    {
        var result = TypeConverter.ConvertTo("7.89", typeof(decimal));
        Assert.Equal(7.89m, result);
    }

    [Fact]
    public void ConvertTo_UnsignedTypes_ReturnsCorrectValue()
    {
        Assert.Equal((ushort)100, TypeConverter.ConvertTo(100, typeof(ushort)));
        Assert.Equal((uint)200, TypeConverter.ConvertTo(200, typeof(uint)));
        Assert.Equal((ulong)300, TypeConverter.ConvertTo(300, typeof(ulong)));
    }

    [Fact]
    public void ConvertTo_SignedByteTypes_ReturnsCorrectValue()
    {
        Assert.Equal((sbyte)-50, TypeConverter.ConvertTo(-50, typeof(sbyte)));
    }

    #region TryConvertTo Additional Tests

    [Fact]
    public void TryConvertTo_WithDBNullValue_ReturnsTrueWithNull()
    {
        var success = TypeConverter.TryConvertTo(DBNull.Value, typeof(int), out var result);
        Assert.True(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_WithExactTypeMatch_ReturnsTrueAndSameValue()
    {
        var original = 42;
        var success = TypeConverter.TryConvertTo(original, typeof(int), out var result);
        Assert.True(success);
        Assert.Equal(original, result);
    }

    [Fact]
    public void TryConvertTo_WithNullTargetType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TypeConverter.TryConvertTo("test", null!, out _));
    }

    [Fact]
    public void TryConvertTo_StringToEnum_ReturnsTrueAndEnumValue()
    {
        var success = TypeConverter.TryConvertTo("Friday", typeof(DayOfWeek), out var result);
        Assert.True(success);
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToEnum_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("InvalidDay", typeof(DayOfWeek), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_IntToEnum_ReturnsTrueAndEnumValue()
    {
        var success = TypeConverter.TryConvertTo(5, typeof(DayOfWeek), out var result);
        Assert.True(success);
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void TryConvertTo_DoubleToDateTime_ConvertsOADate()
    {
        var oaDate = new DateTime(2024, 1, 15).ToOADate();
        var success = TypeConverter.TryConvertTo(oaDate, typeof(DateTime), out var result);
        Assert.True(success);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void TryConvertTo_StringToLong_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("9876543210", typeof(long), out var result);
        Assert.True(success);
        Assert.Equal(9876543210L, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToLong_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a number", typeof(long), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToDecimal_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("123.45", typeof(decimal), out var result);
        Assert.True(success);
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToDecimal_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a decimal", typeof(decimal), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToDouble_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("3.14159", typeof(double), out var result);
        Assert.True(success);
        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void TryConvertTo_DoubleToDouble_ReturnsSameValue()
    {
        var success = TypeConverter.TryConvertTo(3.14159, typeof(double), out var result);
        Assert.True(success);
        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void TryConvertTo_FloatToDouble_ReturnsConvertedValue()
    {
        var success = TypeConverter.TryConvertTo(2.5f, typeof(double), out var result);
        Assert.True(success);
        Assert.Equal(2.5, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToDouble_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a double", typeof(double), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToFloat_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("2.5", typeof(float), out var result);
        Assert.True(success);
        Assert.Equal(2.5f, result);
    }

    [Fact]
    public void TryConvertTo_FloatToFloat_ReturnsSameValue()
    {
        var success = TypeConverter.TryConvertTo(2.5f, typeof(float), out var result);
        Assert.True(success);
        Assert.Equal(2.5f, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToFloat_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a float", typeof(float), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToBool_VariousFormats()
    {
        Assert.True(TypeConverter.TryConvertTo("true", typeof(bool), out var r1) && (bool)r1! == true);
        Assert.True(TypeConverter.TryConvertTo("false", typeof(bool), out var r2) && (bool)r2! == false);
        Assert.True(TypeConverter.TryConvertTo("1", typeof(bool), out var r3) && (bool)r3! == true);
        Assert.True(TypeConverter.TryConvertTo("0", typeof(bool), out var r4) && (bool)r4! == false);
        Assert.True(TypeConverter.TryConvertTo("yes", typeof(bool), out var r5) && (bool)r5! == true);
        Assert.True(TypeConverter.TryConvertTo("no", typeof(bool), out var r6) && (bool)r6! == false);
        Assert.True(TypeConverter.TryConvertTo("y", typeof(bool), out var r7) && (bool)r7! == true);
        Assert.True(TypeConverter.TryConvertTo("n", typeof(bool), out var r8) && (bool)r8! == false);
    }

    [Fact]
    public void TryConvertTo_BoolToBool_ReturnsSameValue()
    {
        var success = TypeConverter.TryConvertTo(true, typeof(bool), out var result);
        Assert.True(success);
        Assert.Equal(true, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToBool_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("maybe", typeof(bool), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToDateTime_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("2024-01-15", typeof(DateTime), out var result);
        Assert.True(success);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void TryConvertTo_DateTimeToDateTime_ReturnsSameValue()
    {
        var dt = new DateTime(2024, 1, 15);
        var success = TypeConverter.TryConvertTo(dt, typeof(DateTime), out var result);
        Assert.True(success);
        Assert.Equal(dt, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToDateTime_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a date", typeof(DateTime), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToGuid_ReturnsTrueAndValue()
    {
        var guidString = "12345678-1234-1234-1234-123456789abc";
        var success = TypeConverter.TryConvertTo(guidString, typeof(Guid), out var result);
        Assert.True(success);
        Assert.Equal(Guid.Parse(guidString), result);
    }

    [Fact]
    public void TryConvertTo_GuidToGuid_ReturnsSameValue()
    {
        var guid = Guid.NewGuid();
        var success = TypeConverter.TryConvertTo(guid, typeof(Guid), out var result);
        Assert.True(success);
        Assert.Equal(guid, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToGuid_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a guid", typeof(Guid), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToShort_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("100", typeof(short), out var result);
        Assert.True(success);
        Assert.Equal((short)100, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToShort_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a short", typeof(short), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToByte_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("255", typeof(byte), out var result);
        Assert.True(success);
        Assert.Equal((byte)255, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToByte_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a byte", typeof(byte), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToSByte_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("-50", typeof(sbyte), out var result);
        Assert.True(success);
        Assert.Equal((sbyte)-50, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToSByte_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a sbyte", typeof(sbyte), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToUShort_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("100", typeof(ushort), out var result);
        Assert.True(success);
        Assert.Equal((ushort)100, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToUShort_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a ushort", typeof(ushort), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToUInt_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("200", typeof(uint), out var result);
        Assert.True(success);
        Assert.Equal((uint)200, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToUInt_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a uint", typeof(uint), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_StringToULong_ReturnsTrueAndValue()
    {
        var success = TypeConverter.TryConvertTo("300", typeof(ulong), out var result);
        Assert.True(success);
        Assert.Equal((ulong)300, result);
    }

    [Fact]
    public void TryConvertTo_InvalidStringToULong_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("not a ulong", typeof(ulong), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvertTo_ObjectToString_AlwaysSucceeds()
    {
        var success = TypeConverter.TryConvertTo(12345, typeof(string), out var result);
        Assert.True(success);
        Assert.Equal("12345", result);
    }

    [Fact]
    public void TryConvertTo_ToNullableType_ReturnsValue()
    {
        var success = TypeConverter.TryConvertTo("123", typeof(int?), out var result);
        Assert.True(success);
        Assert.Equal(123, result);
    }

    [Fact]
    public void TryConvertTo_FallbackToChangeType_ReturnsValue()
    {
        var success = TypeConverter.TryConvertTo("A", typeof(char), out var result);
        Assert.True(success);
        Assert.Equal('A', result);
    }

    [Fact]
    public void TryConvertTo_FallbackToChangeType_InvalidConversion_ReturnsFalse()
    {
        var success = TypeConverter.TryConvertTo("invalid", typeof(char), out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    #endregion

    #region ConvertTo Edge Cases

    [Fact]
    public void ConvertTo_InvalidEnumString_ThrowsInvalidCastException()
    {
        Assert.Throws<InvalidCastException>(() => TypeConverter.ConvertTo("InvalidDay", typeof(DayOfWeek)));
    }

    [Fact]
    public void ConvertTo_ToChar_UsesChangeTypeFallback()
    {
        var result = TypeConverter.ConvertTo("A", typeof(char));
        Assert.Equal('A', result);
    }

    [Fact]
    public void ConvertTo_NullableEnum_ReturnsEnumValue()
    {
        var result = TypeConverter.ConvertTo("Friday", typeof(DayOfWeek?));
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertTo_NullToNullableEnum_ReturnsNull()
    {
        var result = TypeConverter.ConvertTo(null, typeof(DayOfWeek?));
        Assert.Null(result);
    }

    #endregion

#pragma warning disable CS0618 // Type or member is obsolete
    #region ConvertToType (Obsolete) Tests

    [Fact]
    public void ConvertToType_StringToInt_ReturnsConvertedValue()
    {
        var result = TypeConverter.ConvertToType("123", typeof(int));
        Assert.Equal(123, result);
    }

    [Fact]
    public void ConvertToType_NullToNullableInt_ReturnsNull()
    {
        var result = TypeConverter.ConvertToType(null, typeof(int?));
        Assert.Null(result);
    }

    [Fact]
    public void ConvertToType_StringToNullableInt_ReturnsValue()
    {
        var result = TypeConverter.ConvertToType("456", typeof(int?));
        Assert.Equal(456, result);
    }

    #endregion
#pragma warning restore CS0618 // Type or member is obsolete
}
