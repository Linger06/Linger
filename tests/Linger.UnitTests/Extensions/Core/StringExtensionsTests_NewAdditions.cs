using System;
namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{
    [Theory]
    [InlineData("HelloWorld", "World", StringComparison.Ordinal, "Hello")] // 精确后缀
    [InlineData("HelloWorld", "world", StringComparison.Ordinal, "HelloWorld")] // 区分大小写不移除
    [InlineData("HelloWorld", "world", StringComparison.OrdinalIgnoreCase, "Hello")] // 忽略大小写移除
    [InlineData("SuffixSuffix", "Suffix", StringComparison.Ordinal, "Suffix")] // 仅移除一次
    [InlineData("Test", "", StringComparison.Ordinal, "Test")]
    [InlineData("", "X", StringComparison.Ordinal, "")]
    public void RemoveSuffixOnce_ShouldReturnExpected(string input, string suffix, StringComparison cmp, string expected)
    {
        var result = input.RemoveSuffixOnce(suffix, cmp);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("__Value__", "__", StringComparison.Ordinal, "Value")] // 前后各一次
    [InlineData("__Value__", "__", StringComparison.OrdinalIgnoreCase, "Value")] 
    [InlineData("__Value_", "__", StringComparison.Ordinal, "Value_")] // 末尾不匹配
    [InlineData("Value__", "__", StringComparison.Ordinal, "Value")] // 仅后缀
    [InlineData("__Value", "__", StringComparison.Ordinal, "Value")] // 仅前缀
    [InlineData("Value", "__", StringComparison.Ordinal, "Value")] // 都不匹配
    public void RemovePrefixAndSuffix_WithComparison(string input, string token, StringComparison cmp, string expected)
    {
        var result = input.RemovePrefixAndSuffix(token, cmp);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc", "a", StringComparison.Ordinal, "abc")] // 已有前缀不变
    [InlineData("bc", "a", StringComparison.Ordinal, "abc")] // 添加前缀
    [InlineData("Value", "VAL", StringComparison.OrdinalIgnoreCase, "Value")] // 忽略大小写前缀存在
    public void EnsureStartsWith_WithComparison(string input, string prefix, StringComparison cmp, string expected)
    {
        var result = input.EnsureStartsWith(prefix, cmp);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc", "c", StringComparison.Ordinal, "abc")] // 已有后缀不变
    [InlineData("ab", "c", StringComparison.Ordinal, "abc")] // 添加后缀
    [InlineData("Value", "E", StringComparison.OrdinalIgnoreCase, "Value")] // 忽略大小写后缀存在
    public void EnsureEndsWith_WithComparison(string input, string suffix, StringComparison cmp, string expected)
    {
        var result = input.EnsureEndsWith(suffix, cmp);
        Assert.Equal(expected, result);
    }
}
