namespace Linger.UnitTests.Extensions.Core
{
    public partial class StringExtensionsTests
    {
        public static TheoryData<string?, string, string> ToSafeStringData()
        {
            return new TheoryData<string?, string, string>
                {
                    { null, "", "" },
                    { null, "default", "default" },
                    { "value", "default", "value" }
                };
        }

        [Theory]
        [MemberData(nameof(ToSafeStringData))]
        public void ToSafeString_ShouldReturnExpectedResult(string? value, string defaultValue, string expected)
        {
            var result = value.ToStringOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, char?> TryToCharData()
        {
            return new TheoryData<string?, bool, char?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "a", true, 'a' }
                };
        }

        public static TheoryData<string?, bool, short> TryToShortData()
        {
            return new TheoryData<string?, bool, short>
                {
                    { null, false, 0 },
                    { " ", false, 0 },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToShortData))]
        public void TryToShort_ShouldReturnExpectedResult(string? value, bool expectedSuccess, short expectedResult)
        {
            var success = value.TryToShort(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, short?, short?> ToShortOrNullData()
        {
            return new TheoryData<string?, short?, short?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortOrNullData))]
        public void ToShortOrNull_ShouldReturnExpectedResult(string? value, short? defaultValue, short? expected)
        {
            var result = value.ToShortOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, short, short> ToShortData()
        {
            return new TheoryData<string?, short, short>
                {
                    { null, 1, 1 },
                    { " ", 1, 1 },
                    { "123", 1, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortData))]
        public void ToShortOrDefault_ShouldReturnExpectedResult(string? value, short defaultValue, short expected)
        {
            var result = value.ToShortOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToShort_ShouldThrowFormatException_WhenFractionalTextIsProvided()
        {
            Assert.Throws<FormatException>(() => "42.5".ToShort());
        }

        [Fact]
        public void ToShort_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ((string?)null).ToShort());
        }

        [Fact]
        public void ToShort_ShouldThrowFormatException_WhenValueIsWhitespace()
        {
            Assert.Throws<FormatException>(() => " ".ToShort());
        }

        [Fact]
        public void ToShort_ShouldThrowOverflowException_WhenValueIsOutOfRange()
        {
            Assert.Throws<OverflowException>(() => "32768".ToShort());
        }

        public static TheoryData<string?, bool, Guid> TryToGuidData()
        {
            return new TheoryData<string?, bool, Guid>
                {
                    { null, false, Guid.Empty },
                    { " ", false, Guid.Empty },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", true, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(TryToGuidData))]
        public void TryToGuid_ShouldReturnExpectedResult(string? value, bool expectedSuccess, Guid expectedResult)
        {
            var success = value.TryToGuid(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, Guid?, Guid?> ToGuidOrNullData()
        {
            return new TheoryData<string?, Guid?, Guid?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", null, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidOrNullData))]
        public void ToGuidOrNull_ShouldReturnExpectedResult(string? value, Guid? defaultValue, Guid? expected)
        {
            var result = value.ToGuidOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Guid, Guid> ToGuidData()
        {
            return new TheoryData<string?, Guid, Guid>
                {
                    { null, Guid.Empty, Guid.Empty },
                    { " ", Guid.Empty, Guid.Empty },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", Guid.Empty, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData))]
        public void ToGuidOrDefault_ShouldReturnExpectedResult(string? value, Guid defaultValue, Guid expected)
        {
            var result = value.ToGuidOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string, Guid> ToGuidData3()
        {
            return new TheoryData<string, Guid>
                {
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData3))]
        public void ToGuid_ShouldReturnExpectedResult(string value, Guid expected)
        {
            var result = value.ToGuid();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToGuid_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ((string?)null).ToGuid());
        }

        [Fact]
        public void ToGuid_ShouldThrowFormatException_WhenValueIsWhitespace()
        {
            Assert.Throws<FormatException>(() => " ".ToGuid());
        }

        public static TheoryData<string?, Stream> ToStreamData()
        {
            return new TheoryData<string?, Stream>
                {
                    { null, Stream.Null },
                    { " ", new MemoryStream(Encoding.UTF8.GetBytes(" ")) },
                    { "test", new MemoryStream(Encoding.UTF8.GetBytes("test")) }
                };
        }

        [Theory]
        [MemberData(nameof(ToStreamData))]
        public void ToStream_ShouldReturnExpectedResult(string? value, Stream expected)
        {
            var result = value.ToStream();
            Assert.True(StreamContentsAreEqual(expected, result));
        }

        private bool StreamContentsAreEqual(Stream expected, Stream actual)
        {
            if (expected.Length != actual.Length)
                return false;

            expected.Position = 0;
            actual.Position = 0;

            for (var i = 0; i < expected.Length; i++)
            {
                if (expected.ReadByte() != actual.ReadByte())
                    return false;
            }

            return true;
        }

        public static TheoryData<string?, bool, int> TryToIntData()
        {
            return new TheoryData<string?, bool, int>
                {
                    { null, false, 0 },
                    { " ", false, 0 },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToIntData))]
        public void TryToInt_ShouldReturnExpectedResult(string? value, bool expectedSuccess, int expectedResult)
        {
            var success = value.TryToInt(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, int?, int?> ToIntOrNullData()
        {
            return new TheoryData<string?, int?, int?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrNullData))]
        public void ToIntOrNull_ShouldReturnExpectedResult(string? value, int? defaultValue, int? expected)
        {
            var result = value.ToIntOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, int, int> ToIntData()
        {
            return new TheoryData<string?, int, int>
                {
                    { null, 0, 0 },
                    { " ", 0, 0 },
                    { "123", 0, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntData))]
        public void ToInt_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
            var result = value.ToIntOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToInt_ShouldThrowFormatException_WhenFractionalTextIsProvided()
        {
            Assert.Throws<FormatException>(() => "42.5".ToInt());
        }

        [Fact]
        public void ToInt_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ((string?)null).ToInt());
        }

        [Fact]
        public void ToInt_ShouldThrowFormatException_WhenValueIsWhitespace()
        {
            Assert.Throws<FormatException>(() => " ".ToInt());
        }

        [Fact]
        public void ToInt_ShouldThrowOverflowException_WhenValueIsOutOfRange()
        {
            Assert.Throws<OverflowException>(() => "2147483648".ToInt());
        }

        [Fact]
        public void ToInt_ShouldConvertWholeNumberDecimalText()
        {
            Assert.Equal(42, "42.0".ToInt());
        }

        [Fact]
        public void ToInt_ShouldConvertScientificWholeNumberText()
        {
            Assert.Equal(100, "1e2".ToInt());
        }

        public static TheoryData<string?, Func<int?>?, int?> ToIntOrNullData2()
        {
            return new TheoryData<string?, Func<int?>?, int?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrNullData2))]
        public void ToIntOrNull_ShouldReturnExpectedResult2(string? value, Func<int?>? defaultValueFunc, int? expected)
        {
            var result = value.ToIntOrNull();
            Assert.Equal(expected, result);
        }

        // New ToIntOrDefault tests
        public static TheoryData<string?, int, int> ToIntOrDefaultData()
        {
            return new TheoryData<string?, int, int>
                {
                    { null, 0, 0 },
                    { " ", 0, 0 },
                    { "123", 0, 123 },
                    { "abc", 42, 42 },
                    { "123", 999, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToIntOrDefault_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
            var result = value.ToIntOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, long> TryToLongData()
        {
            return new TheoryData<string?, bool, long>
                {
                    { null, false, 0L },
                    { " ", false, 0L },
                    { "123", true, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(TryToLongData))]
        public void TryToLong_ShouldReturnExpectedResult(string? value, bool expectedSuccess, long expectedResult)
        {
            var success = value.TryToLong(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, long?, long?> ToLongOrNullData()
        {
            return new TheoryData<string?, long?, long?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongOrNullData))]
        public void ToLongOrNull_ShouldReturnExpectedResult(string? value, long? defaultValue, long? expected)
        {
            var result = value.ToLongOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, long, long> ToLongData()
        {
            return new TheoryData<string?, long, long>
                {
                    { null, 0L, 0L },
                    { " ", 0L, 0L },
                    { "123", 0L, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongData))]
        public void ToLong_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
            var result = value.ToLongOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToLong_ShouldThrowFormatException_WhenFractionalTextIsProvided()
        {
            Assert.Throws<FormatException>(() => "42.5".ToLong());
        }

        [Fact]
        public void ToLong_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ((string?)null).ToLong());
        }

        [Fact]
        public void ToLong_ShouldThrowFormatException_WhenValueIsWhitespace()
        {
            Assert.Throws<FormatException>(() => " ".ToLong());
        }

        [Fact]
        public void ToLong_ShouldThrowOverflowException_WhenValueIsOutOfRange()
        {
            Assert.Throws<OverflowException>(() => "9223372036854775808".ToLong());
        }

        [Fact]
        public void ToLong_ShouldConvertWholeNumberDecimalText()
        {
            Assert.Equal(42L, "42.0".ToLong());
        }

        [Fact]
        public void ToLong_ShouldConvertScientificWholeNumberText()
        {
            Assert.Equal(100L, "1e2".ToLong());
        }

        // New ToLongOrDefault tests
        public static TheoryData<string?, long, long> ToLongOrDefaultData()
        {
            return new TheoryData<string?, long, long>
                {
                    { null, 0L, 0L },
                    { " ", 0L, 0L },
                    { "123", 0L, 123L },
                    { "abc", 42L, 42L },
                    { "9223372036854775807", 0L, 9223372036854775807L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongOrDefaultData))]
        public void ToLongOrDefault_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
            var result = value.ToLongOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, decimal> TryToDecimalData()
        {
            return new TheoryData<string?, bool, decimal>
                {
                    { null, false, 0m },
                    { " ", false, 0m },
                    { "123.45", true, 123.45m }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDecimalData))]
        public void TryToDecimal_ShouldReturnExpectedResult(string? value, bool expectedSuccess, decimal expectedResult)
        {
            var success = value.TryToDecimal(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ToDecimal_ShouldConvertScientificNotation()
        {
            Assert.Equal(100m, "1e2".ToDecimal());
        }

        [Theory]
        [InlineData("123.45", true, 123.45d)]
        [InlineData("1e2", true, 100d)]
        [InlineData("invalid", false, 0d)]
        [InlineData(null, false, 0d)]
        public void TryToDouble_ShouldReturnExpectedResult(string? value, bool expectedSuccess, double expectedResult)
        {
            var success = value.TryToDouble(out var result);

            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("123.45", true, 123.45f)]
        [InlineData("1e2", true, 100f)]
        [InlineData("invalid", false, 0f)]
        [InlineData(null, false, 0f)]
        public void TryToFloat_ShouldReturnExpectedResult(string? value, bool expectedSuccess, float expectedResult)
        {
            var success = value.TryToFloat(out var result);

            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FloatingPointConversions_ShouldSupportStrictNullableAndDefaultConversions()
        {
            Assert.Equal(123.45d, "123.45".ToDouble());
            Assert.Equal(123.45f, "123.45".ToFloat());
            Assert.Null("invalid".ToDoubleOrNull());
            Assert.Null("invalid".ToFloatOrNull());
            Assert.Equal(42.5d, "invalid".ToDoubleOrDefault(42.5d));
            Assert.Equal(42.5f, "invalid".ToFloatOrDefault(42.5f));
        }

        public static TheoryData<string?, bool, DateTime> TryToDateTimeData()
        {
            return new TheoryData<string?, bool, DateTime>
                {
                    { null, false, default },
                    { " ", false, default },
                    { "2023-01-01", true, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDateTimeData))]
        public void TryToDateTime_ShouldReturnExpectedResult(string? value, bool expectedSuccess, DateTime expectedResult)
        {
            var success = value.TryToDateTime(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TryToDateTime_WithUtcRoundtripText_PreservesUtcKind()
        {
            const string value = "2019-01-30T12:01:02Z";

            var success = value.TryToDateTime(out var result);

            Assert.True(success);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), result);
        }

        public static TheoryData<string?, DateTime?, DateTime?> ToDateTimeOrNullData()
        {
            return new TheoryData<string?, DateTime?, DateTime?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "2023-01-01", null, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeOrNullData))]
        public void ToDateTimeOrNull_ShouldReturnExpectedResult(string? value, DateTime? defaultValue, DateTime? expected)
        {
            var result = value.ToDateTimeOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, DateTime, DateTime> ToDateTimeData()
        {
            return new TheoryData<string?, DateTime, DateTime>
                {
                    { null, DateTime.MinValue, DateTime.MinValue },
                    { " ", DateTime.MinValue, DateTime.MinValue },
                    { "2023-01-01", DateTime.MinValue, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData))]
        public void ToDateTime_ShouldReturnExpectedResult(string? value, DateTime defaultValue, DateTime expected)
        {
            var result = value.ToDateTimeOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, DateTime?> ToDateTimeData2()
        {
            return new TheoryData<string?, DateTime?>
                {
                    { null,  null },
                    { " ",  null },
                    { "2023-01-01",  new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData2))]
        public void ToDateTime_ShouldReturnExpectedResult2(string? value, DateTime? expected)
        {
            var result = value.ToDateTimeOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, bool?> TryToBoolData()
        {
            return new TheoryData<string?, bool, bool?>
                {
                    { null, false, false },
                    { " ", false, false },
                    { "true", true, true },
                    { "false", true, false },
                    { "1", true, true },
                    { "0", true, false }
                };
        }

        [Theory]
        [MemberData(nameof(TryToBoolData))]
        public void TryToBool_ShouldReturnExpectedResult(string? value, bool expectedSuccess, bool? expectedResult)
        {
            var success = value.TryToBool(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, bool?, bool?> ToBoolOrNullData()
        {
            return new TheoryData<string?, bool?, bool?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "true", null, true },
                    { "false", null, false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolOrNullData))]
        public void ToBoolOrNull_ShouldReturnExpectedResult(string? value, bool? defaultValue, bool? expected)
        {
            var result = value.ToBoolOrNull();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, bool> ToBoolData()
        {
            return new TheoryData<string?, bool, bool>
                {
                    { null, false, false },
                    { " ", false, false },
                    { "true", false, true },
                    { "false", false, false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolData))]
        public void ToBool_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
            var result = value.ToBoolOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        // New ToBoolOrDefault tests
        public static TheoryData<string?, bool, bool> ToBoolOrDefaultData()
        {
            return new TheoryData<string?, bool, bool>
                {
                    { null, false, false },
                    { " ", false, false },
                    { "true", false, true },
                    { "false", true, false },
                    { "1", false, true },
                    { "0", true, false },
                    { "yes", false, true },
                    { "no", true, false },
                    { "abc", true, true } // Default when conversion fails
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolOrDefaultData))]
        public void ToBoolOrDefault_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
            var result = value.ToBoolOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData("3,14")]
        [InlineData("1,5")]
        [InlineData("1,2345")]
        [InlineData(",123")]
        [InlineData("1,234,")]
        [InlineData("1,,234")]
        public void NumericParsing_WithMalformedThousandsGrouping_Fails(string value)
        {
            Assert.False(value.TryToShort(out _));
            Assert.False(value.TryToInt(out _));
            Assert.False(value.TryToLong(out _));
            Assert.False(value.TryToDecimal(out _));
            Assert.False(value.TryToDouble(out _));
            Assert.False(value.TryToFloat(out _));
            Assert.Throws<FormatException>(() => value.ToInt());
            Assert.Throws<FormatException>(() => value.ToDecimal());
            Assert.Throws<FormatException>(() => value.ToDouble());
        }

        [Theory]
        [InlineData("1,234", 1234)]
        [InlineData("-12,345,678", -12345678)]
        [InlineData(" 1,234 ", 1234)]
        public void TryToInt_WithWellFormedThousandsGrouping_Succeeds(string value, int expected)
        {
            Assert.True(value.TryToInt(out var result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1,234.5", 1234.5)]
        [InlineData("1,234,567.25", 1234567.25)]
        public void TryToDouble_WithWellFormedThousandsGrouping_Succeeds(string value, double expected)
        {
            Assert.True(value.TryToDouble(out var result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1e400")]
        [InlineData("-1e400")]
        public void TryToDouble_WithOverflowingText_Fails(string value)
        {
            Assert.False(value.TryToDouble(out var result));
            Assert.Equal(0d, result);
        }

        [Theory]
        [InlineData("1e400")]
        [InlineData("-1e400")]
        public void ToDouble_WithOverflowingText_ThrowsOverflowException(string value)
        {
            Assert.Throws<OverflowException>(() => value.ToDouble());
        }

        [Theory]
        [InlineData("1e39")]
        [InlineData("-1e39")]
        public void TryToFloat_WithOverflowingText_Fails(string value)
        {
            Assert.False(value.TryToFloat(out var result));
            Assert.Equal(0f, result);
        }

        [Theory]
        [InlineData("1e39")]
        [InlineData("-1e39")]
        public void ToFloat_WithOverflowingText_ThrowsOverflowException(string value)
        {
            Assert.Throws<OverflowException>(() => value.ToFloat());
        }

        [Theory]
        [InlineData("Infinity", double.PositiveInfinity)]
        [InlineData("-Infinity", double.NegativeInfinity)]
        public void TryToDouble_WithInfinitySymbol_Succeeds(string value, double expected)
        {
            Assert.True(value.TryToDouble(out var result));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToBytes_WithEmptyString_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => "".ToBytes());
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void ToBytes_WithNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((string?)null).ToBytes());
        }

        // 末尾符号（"5-"）更可能是录入错误，悄悄转成 -5 会反转符号，所以所有数值类型一律拒绝
        [Theory]
        [InlineData("5-")]
        [InlineData("5+")]
        [InlineData("1,234-")]
        [InlineData("1.5-")]
        [InlineData("1e2-")]
        public void NumericParsing_WithTrailingSign_Fails(string value)
        {
            Assert.False(value.TryToShort(out _));
            Assert.False(value.TryToInt(out _));
            Assert.False(value.TryToLong(out _));
            Assert.False(value.TryToDecimal(out _));
            Assert.False(value.TryToDouble(out _));
            Assert.False(value.TryToFloat(out _));
            Assert.Throws<FormatException>(() => value.ToShort());
            Assert.Throws<FormatException>(() => value.ToInt());
            Assert.Throws<FormatException>(() => value.ToLong());
            Assert.Throws<FormatException>(() => value.ToDecimal());
            Assert.Throws<FormatException>(() => value.ToDouble());
            Assert.Throws<FormatException>(() => value.ToFloat());
        }

        [Fact]
        public void NumericParsing_WithLeadingSignAndGrouping_StillSucceeds()
        {
            Assert.Equal(-1234, "-1,234".ToInt());
            Assert.Equal(-1234.5m, "-1,234.5".ToDecimal());
            Assert.Equal(-1234.5d, "-1,234.5".ToDouble());
            Assert.Equal(-1234.5f, "-1,234.5".ToFloat());
            Assert.Equal(1234, "+1,234".ToInt());
        }

        [Fact]
        public void ToInt_WithInvalidText_PreservesOriginalExceptionAsInnerException()
        {
            var exception = Assert.Throws<FormatException>(() => "abc".ToInt());
            Assert.IsType<FormatException>(exception.InnerException);
        }

        // 这些写法原先由 DateTimeConversionHelper 里的固定格式列表兜底，锁定它们以确认 InvariantCulture 的通用解析已经覆盖
        [Theory]
        [InlineData("1/5/2024 3:04:05 PM", 2024, 1, 5)]
        [InlineData("12/25/2024 03:04:05 PM", 2024, 12, 25)]
        [InlineData("2024/1/5 15:04:05", 2024, 1, 5)]
        [InlineData("2024/12/25 3:04:05 PM", 2024, 12, 25)]
        [InlineData("2024-01-05 15:04:05", 2024, 1, 5)]
        [InlineData("2024/12/25 15:04:05", 2024, 12, 25)]
        public void TryToDateTime_WithCommonLayouts_Succeeds(string value, int year, int month, int day)
        {
            Assert.True(value.TryToDateTime(out var result));
            Assert.Equal(new DateTime(year, month, day, 15, 4, 5), result);
        }
    }
}
