using Linger.Extensions.Core;

namespace Linger.UnitTests.Extensions.Core
{
    /// <summary>
    /// Tests to ensure backward compatibility and consistent behavior between old and new APIs
    /// </summary>
    public class ApiCompatibilityTests
    {
        public static TheoryData<string?, int, int> StringToIntTestData()
        {
            return new TheoryData<string?, int, int>
            {
                { null, 0, 0 },
                { "", 42, 42 },
                { " ", 42, 42 },
                { "123", 0, 123 },
                { "abc", 99, 99 },
                { "123.45", 0, 0 }, // Should fail conversion
                { "-456", 0, -456 }
            };
        }

        [Theory]
        [MemberData(nameof(StringToIntTestData))]
        public void StringToInt_NewAndOldAPI_ShouldProduceSameResults(string? input, int defaultValue, int expected)
        {
            // Test new API
            var newResult = input.ToIntOrDefault(defaultValue);
            
            // Test old API (with obsolete suppression)
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult = input.ToInt(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(expected, newResult);
            Assert.Equal(expected, oldResult);
            Assert.Equal(newResult, oldResult); // Ensure consistency
        }

        public static TheoryData<string?, long, long> StringToLongTestData()
        {
            return new TheoryData<string?, long, long>
            {
                { null, 0L, 0L },
                { "123", 0L, 123L },
                { "abc", 99L, 99L },
                { "9223372036854775807", 0L, 9223372036854775807L }
            };
        }

        [Theory]
        [MemberData(nameof(StringToLongTestData))]
        public void StringToLong_NewAndOldAPI_ShouldProduceSameResults(string? input, long defaultValue, long expected)
        {
            var newResult = input.ToLongOrDefault(defaultValue);
            
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult = input.ToLong(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(expected, newResult);
            Assert.Equal(expected, oldResult);
            Assert.Equal(newResult, oldResult);
        }

        public static TheoryData<string?, double, double> StringToDoubleTestData()
        {
            return new TheoryData<string?, double, double>
            {
                { null, 0.0, 0.0 },
                { "123.45", 0.0, 123.45 },
                { "abc", 99.9, 99.9 }
            };
        }

        [Theory]
        [MemberData(nameof(StringToDoubleTestData))]
        public void StringToDouble_NewAndOldAPI_ShouldProduceSameResults(string? input, double defaultValue, double expected)
        {
            var newResult = input.ToDoubleOrDefault(defaultValue);
            
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult = input.ToDouble(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(expected, newResult);
            Assert.Equal(expected, oldResult);
            Assert.Equal(newResult, oldResult);
        }

        public static TheoryData<string?, bool, bool> StringToBoolTestData()
        {
            return new TheoryData<string?, bool, bool>
            {
                { null, false, false },
                { "true", false, true },
                { "false", true, false },
                { "1", false, true },
                { "0", true, false },
                { "yes", false, true },
                { "no", true, false },
                { "abc", true, true }
            };
        }

        [Theory]
        [MemberData(nameof(StringToBoolTestData))]
        public void StringToBool_NewAndOldAPI_ShouldProduceSameResults(string? input, bool defaultValue, bool expected)
        {
            var newResult = input.ToBoolOrDefault(defaultValue);
            
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult = input.ToBool(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(expected, newResult);
            Assert.Equal(expected, oldResult);
            Assert.Equal(newResult, oldResult);
        }

        public static TheoryData<object, int, int> ObjectToIntTestData()
        {
            return new TheoryData<object, int, int>
            {
                { "123", 0, 123 },
                { null, 42, 42 },
                { "abc", 99, 99 },
                { 123.45, 88, 88 }
            };
        }

        [Theory]
        [MemberData(nameof(ObjectToIntTestData))]
        public void ObjectToInt_NewAndOldAPI_ShouldProduceSameResults(object input, int defaultValue, int expected)
        {
            var newResult = input.ToIntOrDefault(defaultValue);
            
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult = input.ToInt(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(expected, newResult);
            Assert.Equal(expected, oldResult);
            Assert.Equal(newResult, oldResult);
        }

        [Fact]
        public void ToStringOrDefault_vs_ToSafeString_ShouldProduceSameResults()
        {
            string? nullString = null;
            string testString = "test";
            string defaultValue = "default";

            // Test new API
            var newResult1 = nullString.ToStringOrDefault(defaultValue);
            var newResult2 = testString.ToStringOrDefault(defaultValue);

            // Test old API
#pragma warning disable CS0618 // Type or member is obsolete
            var oldResult1 = nullString.ToSafeString(defaultValue);
            var oldResult2 = testString.ToSafeString(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(defaultValue, newResult1);
            Assert.Equal(defaultValue, oldResult1);
            Assert.Equal(newResult1, oldResult1);

            Assert.Equal(testString, newResult2);
            Assert.Equal(testString, oldResult2);
            Assert.Equal(newResult2, oldResult2);
        }

        [Fact]
        public void AllObsoleteMethodsStillWork()
        {
            // This test ensures that all obsolete methods are still functional
            // and haven't been accidentally broken during refactoring

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Equal(123, "123".ToInt(0));
            Assert.Equal(123L, "123".ToLong(0L));
            Assert.Equal(123.0, "123".ToDouble(0.0));
            Assert.Equal(123.0f, "123".ToFloat(0.0f));
            Assert.Equal(123m, "123".ToDecimal(0m));
            Assert.True("true".ToBool(false));
            Assert.Equal(new DateTime(2023, 1, 1), "2023-01-01".ToDateTime(DateTime.MinValue));
            Assert.Equal("test", "test".ToSafeString("default"));

            // Object extensions
            Assert.Equal(123, ((object)"123").ToInt(0));
            Assert.Equal(123L, ((object)"123").ToLong(0L));
            Assert.Equal(123.0, ((object)"123").ToDouble(0.0));
            Assert.Equal(123.0f, ((object)"123").ToFloat(0.0f));
            Assert.Equal(123m, ((object)"123").ToDecimal(0m));
            Assert.True(((object)"true").ToBool(false));
            Assert.Equal(new DateTime(2023, 1, 1), ((object)"2023-01-01").ToDateTime(DateTime.MinValue));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
