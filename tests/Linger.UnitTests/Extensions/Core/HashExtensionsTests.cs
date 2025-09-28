using Xunit.v3;
using Linger.Extensions.Core;

namespace Linger.Extensions.Tests;

public class HashExtensionsTests
{
    [Fact]
    public void ToMd5HashCode_String_ReturnsLowercaseHex()
    {
        // Arrange
        var input = "abc";
        // The MD5 of "abc" is 900150983cd24fb0d6963f7d28e17f72
        // (lowercase expected)
        var expected = "900150983cd24fb0d6963f7d28e17f72";

        // Act
        var actual = input.ToMd5HashCode();

        // Assert
        Assert.Equal(expected, actual.ToLowerInvariant());
        Assert.Equal(actual, actual.ToLowerInvariant());
    }

    [Fact]
    public void ToSha256HashCode_String_ReturnsLowercaseHex()
    {
        // Arrange
        var input = "abc";
        // The SHA256 of "abc" is
        // ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad
        var expected = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        // Act
        var actual = input.ToSha256HashCode();

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(actual, actual.ToLowerInvariant());
    }
}
