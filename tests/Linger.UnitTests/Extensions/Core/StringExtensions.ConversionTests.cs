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

        // Test backward compatibility - ensure old method still works
        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToInt_BackwardCompatibility_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToIntOrDefault(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
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

        [Theory]
        [MemberData(nameof(ToLongOrDefaultData))]
        public void ToLong_BackwardCompatibility_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToLongOrDefault(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
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
        [MemberData(nameof(ToBoolOrDefaultData))]
        public void ToBool_BackwardCompatibility_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToBoolOrDefault(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }
    }
}
