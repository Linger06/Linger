using System;
using Linger.Extensions.Core;
using Xunit;

namespace Linger.UnitTests.Extensions.Core
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void IsNotNull_ShouldReturnTrue_WhenObjectIsNotNull()
        {
            var obj = new object();
            Assert.True(obj.IsNotNull());
        }

        [Fact]
        public void IsNotNull_ShouldReturnFalse_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.False(obj.IsNotNull());
        }

        [Fact]
        public void IsNull_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNull());
        }

        [Fact]
        public void IsNull_ShouldReturnFalse_WhenObjectIsNotNull()
        {
            var obj = new object();
            Assert.False(obj.IsNull());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnTrue_WhenObjectIsNotNullAndNotEmpty()
        {
            object obj = "Hello";
            Assert.True(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnFalse_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.False(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnFalse_WhenObjectIsEmptyString()
        {
            object obj = "";
            Assert.False(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenObjectIsEmptyString()
        {
            object obj = "";
            Assert.True(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenObjectIsNotNullAndNotEmpty()
        {
            object obj = "Hello";
            Assert.False(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNullOrDbNull());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnTrue_WhenObjectIsDbNull()
        {
            object obj = DBNull.Value;
            Assert.True(obj.IsNullOrDbNull());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnFalse_WhenObjectIsNotNullAndNotDbNull()
        {
            object obj = "Hello";
            Assert.False(obj.IsNullOrDbNull());
        }

        [Fact]
        public void ForIn_ShouldExecuteActionOnEachProperty()
        {
            var obj = new { Name = "John", Age = 30 };
            var properties = new Dictionary<string, object?>();

            obj.ForIn((name, val) => properties[name] = val);

            Assert.Equal(2, properties.Count);
            Assert.Equal("John", properties["Name"]);
            Assert.Equal(30, properties["Age"]);
        }

        [Fact]
        public void ForIn_ShouldNotThrow_WhenObjectIsNull()
        {
            object? obj = null;
            var properties = new Dictionary<string, object?>();

            obj.ForIn((name, val) => properties[name] = val);

            Assert.Empty(properties);
        }

        [Fact]
        public void GetPropertyInfo_ShouldReturnPropertyInfo_WhenPropertyExists()
        {
            var obj = new { Name = "John" };
            var propertyInfo = obj.GetPropertyInfo("Name");

            Assert.NotNull(propertyInfo);
            Assert.Equal("Name", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyInfo_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Throws<InvalidOperationException>(() => obj.GetPropertyInfo("Age"));
        }

        [Fact]
        public void GetPropertyValue_ShouldReturnValue_WhenPropertyExists()
        {
            var obj = new { Name = "John" };
            var value = obj.GetPropertyValue("Name");

            Assert.Equal("John", value);
        }

        [Fact]
        public void GetPropertyValue_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Throws<InvalidOperationException>(() => obj.GetPropertyValue("Age"));
        }

        [Fact]
        public void IsString_ShouldReturnTrue_WhenObjectIsString()
        {
            object str = "Hello";
            Assert.True(str.IsString());
        }

        [Fact]
        public void IsString_ShouldReturnFalse_WhenObjectIsNotString()
        {
            object num = 123;
            Assert.False(num.IsString());
        }

        [Fact]
        public void IsInt16_ShouldReturnTrue_WhenObjectIsInt16()
        {
            object num = (short)123;
            Assert.True(num.IsInt16());
        }

        [Fact]
        public void IsInt16_ShouldReturnFalse_WhenObjectIsNotInt16()
        {
            object num = 123;
            Assert.False(num.IsInt16());
        }

        [Fact]
        public void IsInt_ShouldReturnTrue_WhenObjectIsInt()
        {
            object num = 123;
            Assert.True(num.IsInt());
        }

        [Fact]
        public void IsInt_ShouldReturnFalse_WhenObjectIsNotInt()
        {
            object num = (short)123;
            Assert.False(num.IsInt());
        }

        [Fact]
        public void IsInt64_ShouldReturnTrue_WhenObjectIsInt64()
        {
            object num = 123L;
            Assert.True(num.IsInt64());
        }

        [Fact]
        public void IsInt64_ShouldReturnFalse_WhenObjectIsNotInt64()
        {
            object num = 123;
            Assert.False(num.IsInt64());
        }

        [Fact]
        public void IsDecimal_ShouldReturnTrue_WhenObjectIsDecimal()
        {
            object num = 123.45M;
            Assert.True(num.IsDecimal());
        }

        [Fact]
        public void IsDecimal_ShouldReturnFalse_WhenObjectIsNotDecimal()
        {
            object num = 123.45;
            Assert.False(num.IsDecimal());
        }

        [Fact]
        public void IsSingle_ShouldReturnTrue_WhenObjectIsSingle()
        {
            object num = 123.45F;
            Assert.True(num.IsSingle());
        }

        [Fact]
        public void IsSingle_ShouldReturnFalse_WhenObjectIsNotSingle()
        {
            object num = 123.45;
            Assert.False(num.IsSingle());
        }

        [Fact]
        public void IsFloat_ShouldReturnTrue_WhenObjectIsFloat()
        {
            object num = 123.45F;
            Assert.True(num.IsFloat());
        }

        [Fact]
        public void IsFloat_ShouldReturnFalse_WhenObjectIsNotFloat()
        {
            object num = "Hello";
            Assert.False(num.IsFloat());
        }

        [Fact]
        public void IsDouble_ShouldReturnTrue_WhenObjectIsDouble()
        {
            object num = 123.45;
            Assert.True(num.IsDouble());
        }

        [Fact]
        public void IsDouble_ShouldReturnFalse_WhenObjectIsNotDouble()
        {
            object num = 123;
            Assert.False(num.IsDouble());
        }

        [Fact]
        public void IsDateTime_ShouldReturnTrue_WhenObjectIsDateTime()
        {
            object date = DateTime.Now;
            Assert.True(date.IsDateTime());
        }

        [Fact]
        public void IsDateTime_ShouldReturnFalse_WhenObjectIsNotDateTime()
        {
            object num = 123;
            Assert.False(num.IsDateTime());
        }

        [Fact]
        public void IsBoolean_ShouldReturnTrue_WhenObjectIsBoolean()
        {
            object flag = true;
            Assert.True(flag.IsBoolean());
        }

        [Fact]
        public void IsBoolean_ShouldReturnFalse_WhenObjectIsNotBoolean()
        {
            object num = 123;
            Assert.False(num.IsBoolean());
        }

        [Fact]
        public void IsGuid_ShouldReturnTrue_WhenObjectIsGuid()
        {
            object guid = Guid.NewGuid();
            Assert.True(guid.IsGuid());
        }

        [Fact]
        public void IsGuid_ShouldReturnFalse_WhenObjectIsNotGuid()
        {
            object str = "not-a-guid";
            Assert.False(str.IsGuid());
        }

        [Fact]
        public void IsByte_ShouldReturnTrue_WhenObjectIsByte()
        {
            object num = (byte)255;
            Assert.True(num.IsByte());
        }

        [Fact]
        public void IsByte_ShouldReturnFalse_WhenObjectIsNotByte()
        {
            object num = 256; // int
            Assert.False(num.IsByte());
        }

        [Fact]
        public void IsSByte_ShouldReturnTrue_WhenObjectIsSByte()
        {
            object num = (sbyte)-100;
            Assert.True(num.IsSByte());
        }

        [Fact]
        public void IsSByte_ShouldReturnFalse_WhenObjectIsNotSByte()
        {
            object num = (byte)100;
            Assert.False(num.IsSByte());
        }

        [Fact]
        public void IsUShort_ShouldReturnTrue_WhenObjectIsUShort()
        {
            object num = (ushort)65535;
            Assert.True(num.IsUShort());
        }

        [Fact]
        public void IsUShort_ShouldReturnFalse_WhenObjectIsNotUShort()
        {
            object num = (short)32767;
            Assert.False(num.IsUShort());
        }

        [Fact]
        public void IsUInt_ShouldReturnTrue_WhenObjectIsUInt()
        {
            object num = (uint)4294967295;
            Assert.True(num.IsUInt());
        }

        [Fact]
        public void IsUInt_ShouldReturnFalse_WhenObjectIsNotUInt()
        {
            object num = -1; // int
            Assert.False(num.IsUInt());
        }

        [Fact]
        public void IsULong_ShouldReturnTrue_WhenObjectIsULong()
        {
            object num = (ulong)18446744073709551615;
            Assert.True(num.IsULong());
        }

        [Fact]
        public void IsULong_ShouldReturnFalse_WhenObjectIsNotULong()
        {
            object num = -1L; // long
            Assert.False(num.IsULong());
        }

        [Fact]
        public void IsNumeric_ShouldReturnTrue_WhenObjectIsUnsignedInteger()
        {
            Assert.True(((byte)255).IsNumeric());
            Assert.True(((ushort)65535).IsNumeric());
            Assert.True(((uint)4294967295).IsNumeric());
            Assert.True(((ulong)18446744073709551615).IsNumeric());
            Assert.True(((sbyte)-100).IsNumeric());
        }

        [Fact]
        public void IsAnyUnsignedInteger_ShouldReturnTrue_WhenObjectIsUnsignedInteger()
        {
            Assert.True(((byte)255).IsAnyUnsignedInteger());
            Assert.True(((ushort)65535).IsAnyUnsignedInteger());
            Assert.True(((uint)4294967295).IsAnyUnsignedInteger());
            Assert.True(((ulong)18446744073709551615).IsAnyUnsignedInteger());
            
            // Should return false for signed integers
            Assert.False(((sbyte)-100).IsAnyUnsignedInteger());
            Assert.False(((short)-100).IsAnyUnsignedInteger());
            Assert.False((-100).IsAnyUnsignedInteger());
            Assert.False((-100L).IsAnyUnsignedInteger());
        }

        public static TheoryData<object, string> ToNotSpaceStringData()
        {
            return new TheoryData<object, string>
                {
                    { "  test  ", "test" },
                    { null, string.Empty },
                    { "  ", string.Empty },
                    { "test", "test" }
                };
        }

        [Theory]
        [MemberData(nameof(ToNotSpaceStringData))]
        public void ToNotSpaceString_ShouldReturnExpectedResult(object input, string expected)
        {
            var result = input.ToNotSpaceString();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, string, string> ToSafeStringData()
        {
            return new TheoryData<object, string, string>
                {
                    { "test", "", "test" },
                    { null, "default", "default" },
                    { 123, "", "123" },
                    { null, "", "" }
                };
        }

        [Theory]
        [MemberData(nameof(ToSafeStringData))]
        public void ToSafeString_ShouldReturnExpectedResult(object input, string defaultValue, string expected)
        {
            var result = input.ToSafeString(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, string> ToStringOrEmptyData()
        {
            return new TheoryData<object, string>
                {
                    { "test", "test" },
                    { null, string.Empty },
                    { 123, "123" }
                };
        }

        [Theory]
        [MemberData(nameof(ToStringOrEmptyData))]
        public void ToStringOrEmpty_ShouldReturnExpectedResult(object input, string expected)
        {
            var result = input.ToStringOrEmpty();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, string> ToStringOrNullData()
        {
            return new TheoryData<object, string>
                {
                    { "test", "test" },
                    { null, null },
                    { 123, "123" }
                };
        }

        [Theory]
        [MemberData(nameof(ToStringOrNullData))]
        public void ToStringOrNull_ShouldReturnExpectedResult(object input, string expected)
        {
            var result = input.ToStringOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, short> ToShortData()
        {
            return new TheoryData<object, short>
                {
                    { "123", 123 },
                    { null, 0 },
                    { "invalid", 0 },
                    { 123.45, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortData))]
        public void ToShort_ShouldReturnExpectedResult(object input, short expected)
        {
            var result = input.ToShort();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, short?> ToShortOrNullData()
        {
            return new TheoryData<object, short?>
                {
                    { "123", (short?)123 },
                    { null, null },
                    { "invalid", null },
                    { 123.45, null }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortOrNullData))]
        public void ToShortOrNull_ShouldReturnExpectedResult(object input, short? expected)
        {
            var result = input.ToShortOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, long> ToLongData()
        {
            return new TheoryData<object, long>
                {
                    { "123456789", 123456789L },
                    { null, 0L },
                    { "invalid", 0L },
                    { 123.45, 0L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongData))]
        public void ToLong_ShouldReturnExpectedResult(object input, long expected)
        {
            var result = input.ToLong();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, long?> ToLongOrNullData()
        {
            return new TheoryData<object, long?>
                {
                    { "123456789", 123456789L },
                    { null, null },
                    { "invalid", null },
                    { 123.45, null }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongOrNullData))]
        public void ToLongOrNull_ShouldReturnExpectedResult(object input, long? expected)
        {
            var result = input.ToLongOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, decimal> ToDecimalData()
        {
            return new TheoryData<object, decimal>
                {
                    { "123.45", 123.45m },
                    { null, 0m },
                    { "invalid", 0m },
                    { 123, 123m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalData))]
        public void ToDecimal_ShouldReturnExpectedResult(object input, decimal expected)
        {
            var result = input.ToDecimal();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, decimal?> ToDecimalOrNullData()
        {
            return new TheoryData<object, decimal?>
                {
                    { "123.45", 123.45m },
                    { null, null },
                    { "invalid", null },
                    { 123, 123m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalOrNullData))]
        public void ToDecimalOrNull_ShouldReturnExpectedResult(object input, decimal? expected)
        {
            var result = input.ToDecimalOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, int> ToIntData()
        {
            return new TheoryData<object, int>
                {
                    { "123", 123 },
                    { null, 0 },
                    { "invalid", 0 },
                    { 123.45, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntData))]
        public void ToInt_ShouldReturnExpectedResult(object input, int expected)
        {
            var result = input.ToInt();
            Assert.Equal(expected, result);
        }

        // New ToIntOrDefault tests for ObjectExtensions
        public static TheoryData<object, int, int> ToIntOrDefaultData()
        {
            return new TheoryData<object, int, int>
                {
                    { "123", 0, 123 },
                    { null, 42, 42 },
                    { "invalid", 99, 99 },
                    { 123.45, 88, 88 },
                    { true, 0, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToIntOrDefault_ShouldReturnExpectedResult(object input, int defaultValue, int expected)
        {
            var result = input.ToIntOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        // Test backward compatibility
        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToInt_BackwardCompatibility_ShouldReturnExpectedResult(object input, int defaultValue, int expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = input.ToInt(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, int?> ToIntOrNullData()
        {
            return new TheoryData<object, int?>
                {
                    { "123", 123 },
                    { null, null },
                    { "invalid", null },
                    { 123.45, null }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrNullData))]
        public void ToIntOrNull_ShouldReturnExpectedResult(object input, int? expected)
        {
            var result = input.ToIntOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, double> ToDoubleData()
        {
            return new TheoryData<object, double>
                {
                    { "123.45", 123.45 },
                    { null, 0.0 },
                    { "invalid", 0.0 },
                    { 123, 123.0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleData))]
        public void ToDouble_ShouldReturnExpectedResult(object input, double expected)
        {
            var result = input.ToDouble();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, double?> ToDoubleOrNullData()
        {
            return new TheoryData<object, double?>
                {
                    { "123.45", 123.45 },
                    { null, null },
                    { "invalid", null },
                    { 123, 123.0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleOrNullData))]
        public void ToDoubleOrNull_ShouldReturnExpectedResult(object input, double? expected)
        {
            var result = input.ToDoubleOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, float> ToFloatData()
        {
            return new TheoryData<object, float>
                {
                    { "123.45", 123.45f },
                    { null, 0f },
                    { "invalid", 0f },
                    { 123, 123f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatData))]
        public void ToFloat_ShouldReturnExpectedResult(object input, float expected)
        {
            var result = input.ToFloat();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, float?> ToFloatOrNullData()
        {
            return new TheoryData<object, float?>
                {
                    { "123.45", 123.45f },
                    { null, null },
                    { "invalid", null },
                    { 123, 123f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatOrNullData))]
        public void ToFloatOrNull_ShouldReturnExpectedResult(object input, float? expected)
        {
            var result = input.ToFloatOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, DateTime> ToDateTimeData()
        {
            return new TheoryData<object, DateTime>
                {
                    { "2023-01-01", new DateTime(2023, 1, 1) },
                    { null, DateTime.MinValue },
                    { "invalid", DateTime.MinValue }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData))]
        public void ToDateTime_ShouldReturnExpectedResult(object input, DateTime expected)
        {
            var result = input.ToDateTime();
            Assert.Equal(expected, result);
        }

        // New ToDateTimeOrDefault tests for ObjectExtensions
        public static TheoryData<object, DateTime, DateTime> ToDateTimeOrDefaultData()
        {
            return new TheoryData<object, DateTime, DateTime>
                {
                    { "2023-01-01", DateTime.MinValue, new DateTime(2023, 1, 1) },
                    { null, new DateTime(2020, 1, 1), new DateTime(2020, 1, 1) },
                    { "invalid", new DateTime(2020, 1, 1), new DateTime(2020, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeOrDefaultData))]
        public void ToDateTimeOrDefault_ShouldReturnExpectedResult(object input, DateTime defaultValue, DateTime expected)
        {
            var result = input.ToDateTimeOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ToDateTimeOrDefaultData))]
        public void ToDateTime_BackwardCompatibility_ShouldReturnExpectedResult(object input, DateTime defaultValue, DateTime expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = input.ToDateTime(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, DateTime?> ToDateTimeOrNullData()
        {
            return new TheoryData<object, DateTime?>
                {
                    { "2023-01-01", (DateTime?)new DateTime(2023, 1, 1) },
                    { null, null },
                    { "invalid", null }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeOrNullData))]
        public void ToDateTimeOrNull_ShouldReturnExpectedResult(object input, DateTime? expected)
        {
            var result = input.ToDateTimeOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, bool> ToBoolData()
        {
            return new TheoryData<object, bool>
                {
                    { "true", true },
                    { null, false },
                    { "invalid", false },
                    { "false", false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolData))]
        public void ToBool_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.ToBool();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, bool?> ToBoolOrNullData()
        {
            return new TheoryData<object, bool?>
                {
                    { "true", true },
                    { null, null },
                    { "invalid", null },
                    { "false", false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolOrNullData))]
        public void ToBoolOrNull_ShouldReturnExpectedResult(object input, bool? expected)
        {
            var result = input.ToBoolOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, Guid> ToGuidData()
        {
            var guid = Guid.NewGuid();
            return new TheoryData<object, Guid>
                {
                    { guid.ToString(), guid },
                    { null, Guid.Empty },
                    { "invalid", Guid.Empty }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData))]
        public void ToGuid_ShouldReturnExpectedResult(object input, Guid expected)
        {
            var result = input.ToGuid();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, Guid?> ToGuidOrNullData()
        {
            var guid = Guid.NewGuid();
            return new TheoryData<object, Guid?>
                {
                    { guid.ToString(), (Guid?)guid },
                    { null, null },
                    { "invalid", null }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidOrNullData))]
        public void ToGuidOrNull_ShouldReturnExpectedResult(object input, Guid? expected)
        {
            var result = input.ToGuidOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("123", true, 123)]
        [InlineData("invalid", false, 0)]
        [InlineData(null, false, 0)]
        public void TryToInt_ShouldReturnExpected(object? input, bool expectedSuccess, int expectedValue)
        {
            var success = input.TryToInt(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("922337203685", true, 922337203685L)]
        [InlineData("invalid", false, 0L)]
        [InlineData(null, false, 0L)]
        public void TryToLong_ShouldReturnExpected(object? input, bool expectedSuccess, long expectedValue)
        {
            var success = input.TryToLong(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("123.45", true, 123.45)]
        [InlineData("invalid", false, 0)]
        [InlineData(null, false, 0)]
        public void TryToDecimal_ShouldReturnExpected(object? input, bool expectedSuccess, decimal expectedValue)
        {
            var success = input.TryToDecimal(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        #region Byte Tests

        public static TheoryData<object, byte> ToByteOrDefaultData()
        {
            return new TheoryData<object, byte>
            {
                { "255", 255 },
                { "0", 0 },
                { null, 0 },
                { "invalid", 0 },
                { "256", 0 }, // Out of range
                { "-1", 0 },  // Out of range
                { (byte)100, 100 }, // Direct type
                { 200, 200 } // Valid conversion from int to byte
            };
        }

        [Theory]
        [MemberData(nameof(ToByteOrDefaultData))]
        public void ToByteOrDefault_ShouldReturnExpectedResult(object input, byte expected)
        {
            var result = input.ToByteOrDefault();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, byte?> ToByteOrNullData()
        {
            return new TheoryData<object, byte?>
            {
                { "255", (byte?)255 },
                { "0", (byte?)0 },
                { null, null },
                { "invalid", null },
                { "256", null }, // Out of range
                { (byte)100, (byte?)100 }, // Direct type
                { 200, (byte?)200 } // Valid conversion from int to byte
            };
        }

        [Theory]
        [MemberData(nameof(ToByteOrNullData))]
        public void ToByteOrNull_ShouldReturnExpectedResult(object input, byte? expected)
        {
            var result = input.ToByteOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("255", true, 255)]
        [InlineData("0", true, 0)]
        [InlineData("invalid", false, 0)]
        [InlineData("256", false, 0)] // Out of range
        [InlineData(null, false, 0)]
        public void TryToByte_ShouldReturnExpected(object? input, bool expectedSuccess, byte expectedValue)
        {
            var success = input.TryToByte(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void ToByteOrNull_DirectType_ShouldReturnSameValue()
        {
            object input = (byte)123;
            var result = input.ToByteOrNull();
            Assert.Equal((byte)123, result);
        }

        #endregion

        #region SByte Tests

        public static TheoryData<object, sbyte> ToSByteOrDefaultData()
        {
            return new TheoryData<object, sbyte>
            {
                { "127", 127 },
                { "-128", -128 },
                { "0", 0 },
                { null, 0 },
                { "invalid", 0 },
                { "128", 0 }, // Out of range
                { "-129", 0 }, // Out of range
                { (sbyte)-50, -50 }, // Direct type
                { 100, 100 } // Valid conversion from int to sbyte
            };
        }

        [Theory]
        [MemberData(nameof(ToSByteOrDefaultData))]
        public void ToSByteOrDefault_ShouldReturnExpectedResult(object input, sbyte expected)
        {
            var result = input.ToSByteOrDefault();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, sbyte?> ToSByteOrNullData()
        {
            return new TheoryData<object, sbyte?>
            {
                { "127", (sbyte?)127 },
                { "-128", (sbyte?)-128 },
                { "0", (sbyte?)0 },
                { null, null },
                { "invalid", null },
                { "128", null }, // Out of range
                { (sbyte)-50, (sbyte?)-50 }, // Direct type
                { 100, (sbyte?)100 } // Valid conversion from int to sbyte
            };
        }

        [Theory]
        [MemberData(nameof(ToSByteOrNullData))]
        public void ToSByteOrNull_ShouldReturnExpectedResult(object input, sbyte? expected)
        {
            var result = input.ToSByteOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("127", true, 127)]
        [InlineData("-128", true, -128)]
        [InlineData("invalid", false, 0)]
        [InlineData("128", false, 0)] // Out of range
        [InlineData(null, false, 0)]
        public void TryToSByte_ShouldReturnExpected(object? input, bool expectedSuccess, sbyte expectedValue)
        {
            var success = input.TryToSByte(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        #endregion

        #region UShort Tests

        public static TheoryData<object, ushort> ToUShortOrDefaultData()
        {
            return new TheoryData<object, ushort>
            {
                { "65535", 65535 },
                { "0", 0 },
                { null, 0 },
                { "invalid", 0 },
                { "65536", 0 }, // Out of range
                { "-1", 0 }, // Out of range
                { (ushort)12345, 12345 }, // Direct type
                { 70000, 0 } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToUShortOrDefaultData))]
        public void ToUShortOrDefault_ShouldReturnExpectedResult(object input, ushort expected)
        {
            var result = input.ToUShortOrDefault();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, ushort?> ToUShortOrNullData()
        {
            return new TheoryData<object, ushort?>
            {
                { "65535", (ushort?)65535 },
                { "0", (ushort?)0 },
                { null, null },
                { "invalid", null },
                { "65536", null }, // Out of range
                { (ushort)12345, (ushort?)12345 }, // Direct type
                { 70000, null } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToUShortOrNullData))]
        public void ToUShortOrNull_ShouldReturnExpectedResult(object input, ushort? expected)
        {
            var result = input.ToUShortOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("65535", true, 65535)]
        [InlineData("0", true, 0)]
        [InlineData("invalid", false, 0)]
        [InlineData("65536", false, 0)] // Out of range
        [InlineData(null, false, 0)]
        public void TryToUShort_ShouldReturnExpected(object? input, bool expectedSuccess, ushort expectedValue)
        {
            var success = input.TryToUShort(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        #endregion

        #region UInt Tests

        public static TheoryData<object, uint> ToUIntOrDefaultData()
        {
            return new TheoryData<object, uint>
            {
                { "4294967295", 4294967295u },
                { "0", 0u },
                { null, 0u },
                { "invalid", 0u },
                { "4294967296", 0u }, // Out of range
                { "-1", 0u }, // Out of range
                { (uint)123456789, 123456789u }, // Direct type
                { -1, 0u } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToUIntOrDefaultData))]
        public void ToUIntOrDefault_ShouldReturnExpectedResult(object input, uint expected)
        {
            var result = input.ToUIntOrDefault();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, uint?> ToUIntOrNullData()
        {
            return new TheoryData<object, uint?>
            {
                { "4294967295", (uint?)4294967295u },
                { "0", (uint?)0u },
                { null, null },
                { "invalid", null },
                { "4294967296", null }, // Out of range
                { (uint)123456789, (uint?)123456789u }, // Direct type
                { -1, null } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToUIntOrNullData))]
        public void ToUIntOrNull_ShouldReturnExpectedResult(object input, uint? expected)
        {
            var result = input.ToUIntOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("4294967295", true, 4294967295u)]
        [InlineData("0", true, 0u)]
        [InlineData("invalid", false, 0u)]
        [InlineData("4294967296", false, 0u)] // Out of range
        [InlineData(null, false, 0u)]
        public void TryToUInt_ShouldReturnExpected(object? input, bool expectedSuccess, uint expectedValue)
        {
            var success = input.TryToUInt(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        #endregion

        #region ULong Tests

        public static TheoryData<object, ulong> ToULongOrDefaultData()
        {
            return new TheoryData<object, ulong>
            {
                { "18446744073709551615", 18446744073709551615ul },
                { "0", 0ul },
                { null, 0ul },
                { "invalid", 0ul },
                { "-1", 0ul }, // Out of range
                { (ulong)1234567890123456789, 1234567890123456789ul }, // Direct type
                { -1L, 0ul } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToULongOrDefaultData))]
        public void ToULongOrDefault_ShouldReturnExpectedResult(object input, ulong expected)
        {
            var result = input.ToULongOrDefault();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, ulong?> ToULongOrNullData()
        {
            return new TheoryData<object, ulong?>
            {
                { "18446744073709551615", (ulong?)18446744073709551615ul },
                { "0", (ulong?)0ul },
                { null, null },
                { "invalid", null },
                { "-1", null }, // Out of range
                { (ulong)1234567890123456789, (ulong?)1234567890123456789ul }, // Direct type
                { -1L, null } // Wrong type
            };
        }

        [Theory]
        [MemberData(nameof(ToULongOrNullData))]
        public void ToULongOrNull_ShouldReturnExpectedResult(object input, ulong? expected)
        {
            var result = input.ToULongOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("18446744073709551615", true, 18446744073709551615ul)]
        [InlineData("0", true, 0ul)]
        [InlineData("invalid", false, 0ul)]
        [InlineData(null, false, 0ul)]
        public void TryToULong_ShouldReturnExpected(object? input, bool expectedSuccess, ulong expectedValue)
        {
            var success = input.TryToULong(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        #endregion

        #region Performance Tests - Direct Type Conversion

        [Fact]
        public void ToIntOrNull_DirectTypeConversion_ShouldBeFast()
        {
            object input = 12345; // Already an int
            var result = input.ToIntOrNull();
            Assert.Equal(12345, result);
        }

        [Fact]
        public void ToDecimalOrNull_DirectTypeConversion_ShouldBeFast()
        {
            object input = 123.45m; // Already a decimal
            var result = input.ToDecimalOrNull();
            Assert.Equal(123.45m, result);
        }

        [Fact]
        public void ToDoubleOrNull_DirectTypeConversion_ShouldBeFast()
        {
            object input = 123.45; // Already a double
            var result = input.ToDoubleOrNull();
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void ToFloatOrNull_DirectTypeConversion_ShouldBeFast()
        {
            object input = 123.45f; // Already a float
            var result = input.ToFloatOrNull();
            Assert.Equal(123.45f, result);
        }

        [Fact]
        public void ToBoolOrNull_DirectTypeConversion_ShouldBeFast()
        {
            object input = true; // Already a bool
            var result = input.ToBoolOrNull();
            Assert.Equal(true, result);
        }

        [Fact]
        public void ToGuidOrNull_DirectTypeConversion_ShouldBeFast()
        {
            var guid = Guid.NewGuid();
            object input = guid; // Already a Guid
            var result = input.ToGuidOrNull();
            Assert.Equal(guid, result);
        }

        [Fact]
        public void ToDateTimeOrNull_DirectTypeConversion_ShouldBeFast()
        {
            var dateTime = DateTime.Now;
            object input = dateTime; // Already a DateTime
            var result = input.ToDateTimeOrNull();
            Assert.Equal(dateTime, result);
        }

        #endregion

        #region Type Checking Tests for New Types

        [Theory]
        [InlineData((byte)255, true)]
        [InlineData(255, false)] // int, not byte
        [InlineData("255", false)]
        [InlineData(null, false)]
        public void IsByte_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.IsByte();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData((sbyte)-100, true)]
        [InlineData(-100, false)] // int, not sbyte
        [InlineData("-100", false)]
        [InlineData(null, false)]
        public void IsSByte_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.IsSByte();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData((ushort)65535, true)]
        [InlineData(65535, false)] // int, not ushort
        [InlineData("65535", false)]
        [InlineData(null, false)]
        public void IsUShort_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.IsUShort();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData((uint)4294967295, true)]
        [InlineData(4294967295L, false)] // long, not uint
        [InlineData("4294967295", false)]
        [InlineData(null, false)]
        public void IsUInt_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.IsUInt();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData((ulong)18446744073709551615, true)]
        [InlineData(9223372036854775807L, false)] // long, not ulong
        [InlineData("18446744073709551615", false)]
        [InlineData(null, false)]
        public void IsULong_ShouldReturnExpectedResult(object input, bool expected)
        {
            var result = input.IsULong();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsAnyUnsignedInteger_ShouldReturnTrue_ForUnsignedTypes()
        {
            Assert.True(((byte)255).IsAnyUnsignedInteger());
            Assert.True(((ushort)65535).IsAnyUnsignedInteger());
            Assert.True(((uint)4294967295).IsAnyUnsignedInteger());
            Assert.True(((ulong)18446744073709551615).IsAnyUnsignedInteger());
        }

        [Fact]
        public void IsAnyUnsignedInteger_ShouldReturnFalse_ForSignedTypes()
        {
            Assert.False(((sbyte)-100).IsAnyUnsignedInteger());
            Assert.False(((short)-32768).IsAnyUnsignedInteger());
            Assert.False((-2147483648).IsAnyUnsignedInteger());
            Assert.False((-9223372036854775808L).IsAnyUnsignedInteger());
        }

        [Fact]
        public void IsNumeric_ShouldIncludeAllNewNumericTypes()
        {
            // Test that IsNumeric includes all new types
            Assert.True(((byte)255).IsNumeric());
            Assert.True(((sbyte)-100).IsNumeric());
            Assert.True(((ushort)65535).IsNumeric());
            Assert.True(((uint)4294967295).IsNumeric());
            Assert.True(((ulong)18446744073709551615).IsNumeric());
        }

        #endregion

        #region Performance Tests for New Types

        [Fact]
        public void ToByteOrNull_DirectTypeConversion_ShouldBeFast()
        {
            byte originalByte = 200;
            object input = originalByte; // Already a byte
            var result = input.ToByteOrNull();
            Assert.Equal(originalByte, result);
        }

        [Fact]
        public void ToSByteOrNull_DirectTypeConversion_ShouldBeFast()
        {
            sbyte originalSByte = -100;
            object input = originalSByte; // Already a sbyte
            var result = input.ToSByteOrNull();
            Assert.Equal(originalSByte, result);
        }

        [Fact]
        public void ToUShortOrNull_DirectTypeConversion_ShouldBeFast()
        {
            ushort originalUShort = 50000;
            object input = originalUShort; // Already a ushort
            var result = input.ToUShortOrNull();
            Assert.Equal(originalUShort, result);
        }

        [Fact]
        public void ToUIntOrNull_DirectTypeConversion_ShouldBeFast()
        {
            uint originalUInt = 3000000000;
            object input = originalUInt; // Already a uint
            var result = input.ToUIntOrNull();
            Assert.Equal(originalUInt, result);
        }

        [Fact]
        public void ToULongOrNull_DirectTypeConversion_ShouldBeFast()
        {
            ulong originalULong = 15000000000000000000;
            object input = originalULong; // Already a ulong
            var result = input.ToULongOrNull();
            Assert.Equal(originalULong, result);
        }

        #endregion
    }
}
