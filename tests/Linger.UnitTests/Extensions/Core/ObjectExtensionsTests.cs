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
        public void TryInt_ShouldReturnExpected(string? input, bool expectedSuccess, int expectedValue)
        {
            var success = input.TryInt(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("922337203685", true, 922337203685L)]
        [InlineData("invalid", false, 0L)]
        [InlineData(null, false, 0L)]
        public void TryLong_ShouldReturnExpected(string? input, bool expectedSuccess, long expectedValue)
        {
            var success = input.TryLong(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("123.45", true, 123.45)]
        [InlineData("invalid", false, 0)]
        [InlineData(null, false, 0)]
        public void TryDecimal_ShouldReturnExpected(string? input, bool expectedSuccess, decimal expectedValue)
        {
            var success = input.TryDecimal(out var value);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedValue, value);
        }
    }
}
