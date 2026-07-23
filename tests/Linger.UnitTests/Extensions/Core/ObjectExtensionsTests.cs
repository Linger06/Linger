using System;
using System.Globalization;
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
        public void IsNotNullOrEmpty_ShouldReturnTrue_WhenObjectIsNotNullAndNotEmpty()
        {
            object obj = "Hello";
            Assert.True(obj.IsNotNullOrEmpty());
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldReturnFalse_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.False(obj.IsNotNullOrEmpty());
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldReturnFalse_WhenObjectIsEmptyString()
        {
            object obj = "";
            Assert.False(obj.IsNotNullOrEmpty());
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

        [Theory]
        [InlineData(1, true)]
        [InlineData(4, false)]
        public void In_ShouldReturnWhetherValueIsPresent(int value, bool expected)
        {
            Assert.Equal(expected, value.In(1, 2, 3));
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(4, true)]
        public void NotIn_ShouldReturnWhetherValueIsAbsent(int value, bool expected)
        {
            Assert.Equal(expected, value.NotIn(1, 2, 3));
        }

        [Fact]
        public void ForEachProperty_ShouldEnumerateReadableNonIndexedProperties()
        {
            var value = new PropertyEnumerationTestModel { Name = "Linger", Count = 6 };
            var properties = new Dictionary<string, object?>();

            value.ForEachProperty((name, propertyValue) => properties.Add(name, propertyValue));

            Assert.Equal("Linger", properties[nameof(PropertyEnumerationTestModel.Name)]);
            Assert.Equal(6, properties[nameof(PropertyEnumerationTestModel.Count)]);
            Assert.DoesNotContain("Item", properties.Keys);
        }

        [Fact]
        public void ForEachProperty_ShouldNotInvokeAction_WhenValueIsNull()
        {
            object? value = null;
            var invoked = false;

            value.ForEachProperty((_, _) => invoked = true);

            Assert.False(invoked);
        }

        [Fact]
        public void ForIn_ShouldForwardToForEachProperty()
        {
            var value = new PropertyEnumerationTestModel { Name = "Linger" };
            var properties = new Dictionary<string, object?>();

#pragma warning disable CS0618
            value.ForIn((name, propertyValue) => properties.Add(name, propertyValue));
#pragma warning restore CS0618

            Assert.Equal("Linger", properties[nameof(PropertyEnumerationTestModel.Name)]);
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
        public void GetPropertyInfo_ShouldReturnNull_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Null(obj.GetPropertyInfo("Age"));
        }

        [Fact]
        public void GetPropertyValue_ShouldReturnValue_WhenPropertyExists()
        {
            var obj = new { Name = "John" };
            var value = obj.GetPropertyValue("Name");

            Assert.Equal("John", value);
        }

        [Fact]
        public void GetPropertyValue_ShouldReturnNull_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Null(obj.GetPropertyValue("Age"));
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

        public static TheoryData<object, string> ToTrimmedStringData()
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
        [MemberData(nameof(ToTrimmedStringData))]
        public void ToTrimmedString_ShouldReturnExpectedResult(object input, string expected)
        {
            var result = input.ToTrimmedString();
            Assert.Equal(expected, result);
        }

        public static TheoryData<object, string, string> ToStringOrDefaultData()
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
        [MemberData(nameof(ToStringOrDefaultData))]
        public void ToStringOrDefault_ShouldReturnExpectedResult(object input, string defaultValue, string expected)
        {
            var result = input.ToStringOrDefault(defaultValue);
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
        [MemberData(nameof(ToDoubleOrNullData))]
        public void ToDoubleOrNull_ShouldReturnExpectedResult(object input, double? expected)
        {
            var result = input.ToDoubleOrNull();
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ToFloatOrNullData))]
        public void ToFloatOrNull_ShouldReturnExpectedResult(object input, float? expected)
        {
            var result = input.ToFloatOrNull();
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

        [Theory]
        [InlineData((short)123, (short)123)]
        [InlineData(123, (short)123)]
        [InlineData("123", (short)123)]
        [InlineData("42.0", (short)42)]
        [InlineData("1e2", (short)100)]
        public void ToShort_ShouldReturnExpectedResult(object input, short expected)
        {
            var result = input.ToShort();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(" ", typeof(FormatException))]
        [InlineData("invalid", typeof(FormatException))]
        [InlineData("32768", typeof(OverflowException))]
        public void ToShort_ShouldThrowExpectedException(object input, Type expectedExceptionType)
        {
            var exception = Assert.Throws(expectedExceptionType, () => input.ToShort());
            Assert.NotNull(exception);
        }

        [Fact]
        public void ToShort_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToShort());
        }

        [Theory]
        [InlineData(42.5)]
        [InlineData(42.5f)]
        public void ToShort_ShouldThrowInvalidCastException_WhenFractionalNumericInputIsProvided(object input)
        {
            Assert.Throws<InvalidCastException>(() => input.ToShort());
        }

        [Fact]
        public void ToShort_ShouldThrowInvalidCastException_WhenFractionalDecimalInputIsProvided()
        {
            object input = 42.5m;
            Assert.Throws<InvalidCastException>(() => input.ToShort());
        }

        [Theory]
        [InlineData((long)123, 123L)]
        [InlineData(123, 123L)]
        [InlineData("123", 123L)]
        [InlineData("42.0", 42L)]
        [InlineData("1e2", 100L)]
        public void ToLong_ShouldReturnExpectedResult(object input, long expected)
        {
            var result = input.ToLong();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(" ", typeof(FormatException))]
        [InlineData("invalid", typeof(FormatException))]
        [InlineData("9223372036854775808", typeof(OverflowException))]
        public void ToLong_ShouldThrowExpectedException(object input, Type expectedExceptionType)
        {
            var exception = Assert.Throws(expectedExceptionType, () => input.ToLong());
            Assert.NotNull(exception);
        }

        [Fact]
        public void ToLong_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToLong());
        }

        [Theory]
        [InlineData(42.5)]
        [InlineData(42.5f)]
        public void ToLong_ShouldThrowInvalidCastException_WhenFractionalNumericInputIsProvided(object input)
        {
            Assert.Throws<InvalidCastException>(() => input.ToLong());
        }

        [Fact]
        public void ToLong_ShouldThrowInvalidCastException_WhenFractionalDecimalInputIsProvided()
        {
            object input = 42.5m;
            Assert.Throws<InvalidCastException>(() => input.ToLong());
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData("123", 123)]
        [InlineData((short)123, 123)]
        [InlineData("42.0", 42)]
        [InlineData("1e2", 100)]
        public void ToInt_ShouldReturnExpectedResult(object input, int expected)
        {
            var result = input.ToInt();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(" ", typeof(FormatException))]
        [InlineData("invalid", typeof(FormatException))]
        [InlineData("2147483648", typeof(OverflowException))]
        public void ToInt_ShouldThrowExpectedException(object input, Type expectedExceptionType)
        {
            var exception = Assert.Throws(expectedExceptionType, () => input.ToInt());
            Assert.NotNull(exception);
        }

        [Fact]
        public void ToInt_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToInt());
        }

        [Theory]
        [InlineData(42.5)]
        [InlineData(42.5f)]
        public void ToInt_ShouldThrowInvalidCastException_WhenFractionalNumericInputIsProvided(object input)
        {
            Assert.Throws<InvalidCastException>(() => input.ToInt());
        }

        [Fact]
        public void ToInt_ShouldThrowInvalidCastException_WhenFractionalDecimalInputIsProvided()
        {
            object input = 42.5m;
            Assert.Throws<InvalidCastException>(() => input.ToInt());
        }

        [Fact]
        public void ToInt_ShouldUseInvariantCultureInExceptionMessage_ForFractionalDouble()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                object input = 42.5d;
                var exception = Assert.Throws<InvalidCastException>(() => input.ToInt());

                Assert.Contains("42.5", exception.Message);
                Assert.DoesNotContain("42,5", exception.Message);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        public static TheoryData<object, decimal> ToDecimalShouldReturnExpectedData()
        {
            return new TheoryData<object, decimal>
                {
                    { 123.45m, 123.45m },
                    { "123.45", 123.45m },
                    { 123, 123m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalShouldReturnExpectedData))]
        public void ToDecimal_ShouldReturnExpectedResult(object input, decimal expected)
        {
            var result = input.ToDecimal();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("invalid", typeof(FormatException))]
        [InlineData("79228162514264337593543950336", typeof(OverflowException))]
        public void ToDecimal_ShouldThrowExpectedException(object input, Type expectedExceptionType)
        {
            var exception = Assert.Throws(expectedExceptionType, () => input.ToDecimal());
            Assert.NotNull(exception);
        }

        [Fact]
        public void ToDecimal_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToDecimal());
        }

        [Theory]
        [InlineData("123.45", 123.45d)]
        [InlineData(123, 123d)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        public void ToDouble_ShouldReturnExpectedResult(object input, double expected)
        {
            var result = input.ToDouble();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("123.45", 123.45f)]
        [InlineData(123, 123f)]
        [InlineData(float.PositiveInfinity, float.PositiveInfinity)]
        public void ToFloat_ShouldReturnExpectedResult(object input, float expected)
        {
            var result = input.ToFloat();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToDouble_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToDouble());
        }

        [Fact]
        public void ToFloat_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToFloat());
        }

        [Fact]
        public void FloatingPointConversions_ShouldUseDefaultValue_WhenTryConversionFails()
        {
            object input = "invalid";

            Assert.Equal(42.5d, input.ToDoubleOrDefault(42.5d));
            Assert.Equal(42.5f, input.ToFloatOrDefault(42.5f));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void TryToDecimal_ShouldReturnFalse_ForNonFiniteDouble(double input)
        {
            object value = input;

            var success = value.TryToDecimal(out var result);

            Assert.False(success);
            Assert.Equal(0m, result);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void TryToDecimal_ShouldReturnFalse_ForNonFiniteFloat(float input)
        {
            object value = input;

            var success = value.TryToDecimal(out var result);

            Assert.False(success);
            Assert.Equal(0m, result);
        }

        [Theory]
        [InlineData("2023-01-01")]
        public void ToDateTime_ShouldReturnExpectedResult(object input)
        {
            var result = input.ToDateTime();
            Assert.Equal(new DateTime(2023, 1, 1), result);
        }

        [Fact]
        public void ToDateTime_ShouldNormalizeDateTimeOffsetToUtc()
        {
            object input = new DateTimeOffset(2024, 1, 15, 12, 30, 45, TimeSpan.FromHours(8));

            var result = input.ToDateTime();

            Assert.Equal(((DateTimeOffset)input).UtcDateTime, result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Theory]
        [InlineData("invalid")]
        public void ToDateTime_ShouldThrowFormatException_WhenInputIsInvalid(string input)
        {
            Assert.Throws<FormatException>(() => input.ToDateTime());
        }

        [Fact]
        public void ToDateTime_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            object? input = null;

            Assert.Throws<ArgumentNullException>(() => input.ToDateTime());
        }

        [Fact]
        public void ToBool_ShouldUseInvariantCultureInExceptionMessage_ForDecimalInput()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                object input = 2.5m;
                var exception = Assert.Throws<InvalidCastException>(() => input.ToBool());

                Assert.Contains("2.5", exception.Message);
                Assert.DoesNotContain("2,5", exception.Message);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Theory]
        [InlineData(1f, true)]
        [InlineData(0f, false)]
        public void ToBool_ShouldSupportFloatZeroAndOne(float input, bool expected)
        {
            object value = input;

            var result = value.ToBool();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToBool_ShouldThrowInvalidCastException_ForFractionalFloatInput()
        {
            object input = 1.5f;

            Assert.Throws<InvalidCastException>(() => input.ToBool());
        }


        public static TheoryData<object, bool?> ToBoolOrNullData()
        {
            return new TheoryData<object, bool?>
                {
                    { "true", true },
                    { 1f, true },
                    { 0f, false },
                    { 1.5f, null },
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
        [InlineData("42.0", true, 42)]
        [InlineData("1e2", true, 100)]
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
        [InlineData("42.0", true, 42L)]
        [InlineData("1e2", true, 100L)]
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

        [Theory]
        [InlineData("123.45", true, 123.45d)]
        [InlineData("invalid", false, 0d)]
        [InlineData(null, false, 0d)]
        public void TryToDouble_ShouldReturnExpected(object? input, bool expectedSuccess, double expectedValue)
        {
            var success = input.TryToDouble(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("123.45", true, 123.45f)]
        [InlineData("invalid", false, 0f)]
        [InlineData(null, false, 0f)]
        public void TryToFloat_ShouldReturnExpected(object? input, bool expectedSuccess, float expectedValue)
        {
            var success = input.TryToFloat(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void TryToTargetDouble_ShouldUseTheFloatingPointConversionRules()
        {
            object input = 123;

            var success = input.TryToTarget<double>(out var value);

            Assert.True(success);
            Assert.Equal(123d, value);
        }

        private sealed class PropertyEnumerationTestModel
        {
            public string Name { get; init; } = string.Empty;
            public int Count { get; init; }
            public string this[int index] => index.ToString(CultureInfo.InvariantCulture);
        }
    }
}
