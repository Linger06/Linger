namespace Linger.UnitTests.Extensions.Core
{
    public class StringExtensions8Tests
    {
        public static TheoryData<string?, bool, int?> TryToIntData()
        {
            return new TheoryData<string?, bool, int?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToIntData))]
        public void TryToInt_ShouldReturnExpectedResult(string? value, bool expectedSuccess, int? expectedResult)
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
            var result = value.ToIntOrNull(defaultValue);
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
            var result = value.ToInt(defaultValue);
            Assert.Equal(expected, result);
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
            var result = value.ToIntOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<int>?, int> ToIntData2()
        {
            return new TheoryData<string?, Func<int>?, int>
                {
                    { null, ()=>0, 0 },
                    { " ", ()=>0, 0 },
                    { "123",()=> 0, 123 },
                    { null,null, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntData2))]
        public void ToInt_ShouldReturnExpectedResult2(string? value, Func<int>? defaultValueFunc, int expected)
        {
            var result = value.ToInt(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, long?> TryToLongData()
        {
            return new TheoryData<string?, bool, long?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(TryToLongData))]
        public void TryToLong_ShouldReturnExpectedResult(string? value, bool expectedSuccess, long? expectedResult)
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
            var result = value.ToLongOrNull(defaultValue);
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
            var result = value.ToLong(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, decimal?> TryToDecimalData()
        {
            return new TheoryData<string?, bool, decimal?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45m }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDecimalData))]
        public void TryToDecimal_ShouldReturnExpectedResult(string? value, bool expectedSuccess, decimal? expectedResult)
        {
            var success = value.TryToDecimal(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, decimal?, int?, decimal?> ToDecimalOrNullData()
        {
            return new TheoryData<string?, decimal?, int?, decimal?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45m },
                    { "123.456", null, 2, 123.46m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalOrNullData))]
        public void ToDecimalOrNull_ShouldReturnExpectedResult(string? value, decimal? defaultValue, int? digits, decimal? expected)
        {
            var result = value.ToDecimalOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, decimal, int?, decimal> ToDecimalData()
        {
            return new TheoryData<string?, decimal, int?, decimal>
                {
                    { null, 0m, null, 0m },
                    { " ", 0m, null, 0m },
                    { "123.45", 0m, null, 123.45m },
                    { "123.456", 0m, 2, 123.46m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalData))]
        public void ToDecimal_ShouldReturnExpectedResult(string? value, decimal defaultValue, int? digits, decimal expected)
        {
            var result = value.ToDecimal(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, float?> TryToFloatData()
        {
            return new TheoryData<string?, bool, float?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45f }
                };
        }

        [Theory]
        [MemberData(nameof(TryToFloatData))]
        public void TryToFloat_ShouldReturnExpectedResult(string? value, bool expectedSuccess, float? expectedResult)
        {
            var success = value.TryToFloat(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, float?, int?, float?> ToFloatOrNullData()
        {
            return new TheoryData<string?, float?, int?, float?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45f },
                    { "123.456", null, 2, 123.46f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatOrNullData))]
        public void ToFloatOrNull_ShouldReturnExpectedResult(string? value, float? defaultValue, int? digits, float? expected)
        {
            var result = value.ToFloatOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, float, int?, float> ToFloatData()
        {
            return new TheoryData<string?, float, int?, float>
                {
                    { null, 0f, null, 0f },
                    { " ", 0f, null, 0f },
                    { "123.45", 0f, null, 123.45f },
                    { "123.456", 0f, 2, 123.46f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatData))]
        public void ToFloat_ShouldReturnExpectedResult(string? value, float defaultValue, int? digits, float expected)
        {
            var result = value.ToFloat(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, double?> TryToDoubleData()
        {
            return new TheoryData<string?, bool, double?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDoubleData))]
        public void TryToDouble_ShouldReturnExpectedResult(string? value, bool expectedSuccess, double? expectedResult)
        {
            var success = value.TryToDouble(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, double?, int?, double?> ToDoubleOrNullData()
        {
            return new TheoryData<string?, double?, int?, double?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45 },
                    { "123.456", null, 2, 123.46 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleOrNullData))]
        public void ToDoubleOrNull_ShouldReturnExpectedResult(string? value, double? defaultValue, int? digits, double? expected)
        {
            var result = value.ToDoubleOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, double, int?, double> ToDoubleData()
        {
            return new TheoryData<string?, double, int?, double>
                {
                    { null, 0.0, null, 0.0 },
                    { " ", 0.0, null, 0.0 },
                    { "123.45", 0.0, null, 123.45 },
                    { "123.456", 0.0, 2, 123.46 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleData))]
        public void ToDouble_ShouldReturnExpectedResult(string? value, double defaultValue, int? digits, double expected)
        {
            var result = value.ToDouble(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, DateTime?> TryToDateTimeData()
        {
            return new TheoryData<string?, bool, DateTime?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "2023-01-01", true, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDateTimeData))]
        public void TryToDateTime_ShouldReturnExpectedResult(string? value, bool expectedSuccess, DateTime? expectedResult)
        {
            var success = value.TryToDateTime(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
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
            var result = value.ToDateTimeOrNull(defaultValue);
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
            var result = value.ToDateTime(defaultValue);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, DateTime> ToDateTimeData2()
        {
            return new TheoryData<string?, DateTime>
                {
                    { null,  DateTime.MinValue },
                    { " ",  DateTime.MinValue },
                    { "2023-01-01",  new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData2))]
        public void ToDateTime_ShouldReturnExpectedResult2(string? value, DateTime expected)
        {
            var result = value.ToDateTime();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, bool?> TryToBoolData()
        {
            return new TheoryData<string?, bool, bool?>
                {
                    { null, false, null },
                    { " ", false, null },
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
            var result = value.ToBoolOrNull(defaultValue);
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
            var result = value.ToBool(defaultValue);
            Assert.Equal(expected, result);
        }
    }
}
