namespace Linger.UnitTests.Extensions.Core
{
    public class ObjectExtensions3Tests
    {
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
    }
}
