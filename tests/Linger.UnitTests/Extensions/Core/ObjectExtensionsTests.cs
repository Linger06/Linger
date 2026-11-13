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
        [InlineData("123.45", true, "123.45")]
        [InlineData("invalid", false, "0")]
        [InlineData(null, false, "0")]
        public void TryToDecimal_ShouldReturnExpected(object? input, bool expectedSuccess, string expectedValue)
        {
            var success = input.TryToDecimal(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(decimal.Parse(expectedValue, CultureInfo.InvariantCulture), value);
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

        private sealed class PropertyEnumerationTestModel
        {
            public string Name { get; init; } = string.Empty;
            public int Count { get; init; }
            public string this[int index] => index.ToString(CultureInfo.InvariantCulture);
        }

        [Fact]
        public void TryToInt_WithFloatAtTwoToThe31_ReturnsFalse()
        {
            Assert.False(((object)2147483648f).TryToInt(out _));
            Assert.Throws<OverflowException>(() => ((object)2147483648f).ToInt());
        }

        [Fact]
        public void TryToInt_WithFloatsAtTheEdgesOfTheRange_ReturnsExactValues()
        {
            Assert.True(((object)2147483520f).TryToInt(out var largest));
            Assert.Equal(2147483520, largest);
            Assert.True(((object)(-2147483648f)).TryToInt(out var smallest));
            Assert.Equal(int.MinValue, smallest);
        }

        [Fact]
        public void TryToLong_WithDoubleAtTwoToThe63_ReturnsFalse()
        {
            Assert.False(((object)9223372036854775808d).TryToLong(out _));
            Assert.Throws<OverflowException>(() => ((object)9223372036854775808d).ToLong());
        }

        [Fact]
        public void TryToDecimal_WithDoubleAtTwoToThe96_ReturnsFalseWithoutThrowing()
        {
            object value = 79228162514264337593543950336d;

            Assert.False(value.TryToDecimal(out _));
            Assert.Throws<OverflowException>(() => value.ToDecimal());
        }

        public static TheoryData<object, int?, Type?> IntConsistencyData => new()
        {
            { DayOfWeek.Monday, 1, null },
            { true, null, typeof(InvalidCastException) },
            { false, null, typeof(InvalidCastException) },
            { (sbyte)-3, -3, null },
            { (ushort)5, 5, null },
            { (uint)5, 5, null },
            { (ulong)5, 5, null },
            { '5', 5, null },
            { 'A', null, typeof(InvalidCastException) },
            { uint.MaxValue, null, typeof(OverflowException) },
            { ulong.MaxValue, null, typeof(OverflowException) },
        };

        [Theory]
        [MemberData(nameof(IntConsistencyData))]
        public void ToInt_AgreesWithTryToIntAndToIntOrNull(object value, int? expected, Type? expectedException)
        {
            Assert.Equal(expected, value.ToIntOrNull());
            Assert.Equal(expected.HasValue, value.TryToInt(out var tryResult));
            if (expectedException is null)
            {
                Assert.Equal(expected!.Value, tryResult);
                Assert.Equal(expected.Value, value.ToInt());
            }
            else
            {
                Assert.Throws(expectedException, () => value.ToInt());
            }
        }

        [Fact]
        public void LongAndShortFamilies_AgreeOnEnumsAndOverflow()
        {
            Assert.Equal(1L, ((object)DayOfWeek.Monday).ToLong());
            Assert.Equal(1L, ((object)DayOfWeek.Monday).ToLongOrNull());
            Assert.Null(((object)(ushort)40000).ToShortOrNull());
            Assert.Throws<OverflowException>(() => ((object)(ushort)40000).ToShort());
        }

        [Fact]
        public void DecimalFamily_RejectsBooleansAndAcceptsEnums()
        {
            Assert.Throws<InvalidCastException>(() => ((object)true).ToDecimal());
            Assert.Null(((object)true).ToDecimalOrNull());
            Assert.Equal(1m, ((object)DayOfWeek.Monday).ToDecimalOrNull());
        }

        [Fact]
        public void DoubleAndFloatFamilies_AgreeWithStrictConversions()
        {
            Assert.Null(((object)true).ToDoubleOrNull());
            Assert.Throws<InvalidCastException>(() => ((object)true).ToDouble());
            Assert.Equal(1d, ((object)DayOfWeek.Monday).ToDoubleOrNull());
            Assert.Equal(5d, ((object)new System.Numerics.BigInteger(5)).ToDoubleOrNull());
            Assert.Null(((object)1e39).ToFloatOrNull());
            Assert.Throws<OverflowException>(() => ((object)1e39).ToFloat());
            Assert.Throws<InvalidCastException>(() => ((object)'A').ToDouble());
        }

        [Fact]
        public void BoolFamily_AcceptsOnlyZeroAndOneForEveryIntegerType()
        {
            Assert.True(((object)(byte)1).ToBool());
            Assert.True(((object)(ushort)1).ToBoolOrNull());
            Assert.Null(((object)(ushort)5).ToBoolOrNull());
            Assert.Throws<InvalidCastException>(() => ((object)(ushort)5).ToBool());
            Assert.Throws<InvalidCastException>(() => ((object)DayOfWeek.Tuesday).ToBool());
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void WholeNumberFamilies_ThrowOverflowException_ForNonFiniteInput(object input)
        {
            Assert.Throws<OverflowException>(() => input.ToShort());
            Assert.Throws<OverflowException>(() => input.ToInt());
            Assert.Throws<OverflowException>(() => input.ToLong());
        }

        [Fact]
        public void ToGuid_ThrowsInvalidCastException_ForUnsupportedType()
        {
            Assert.Throws<InvalidCastException>(() => ((object)123).ToGuid());
        }

        [Fact]
        public void ToGuid_ThrowsFormatException_ForInvalidString()
        {
            Assert.Throws<FormatException>(() => ((object)"invalid").ToGuid());
        }

        [Fact]
        public void ToGuid_ThrowsFormatException_ForByteArrayWithWrongLength()
        {
            Assert.Throws<FormatException>(() => ((object)new byte[3]).ToGuid());
        }

        [Fact]
        public void ToDateTime_ThrowsInvalidCastException_ForUnsupportedType()
        {
            Assert.Throws<InvalidCastException>(() => ((object)42).ToDateTime());
        }

        [Fact]
        public void ToBool_ThrowsFormatException_ForUnrecognizedString()
        {
            Assert.Throws<FormatException>(() => ((object)"maybe").ToBool());
        }
    }
}
