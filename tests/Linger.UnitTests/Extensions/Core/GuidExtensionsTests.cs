namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class GuidExtensionsTests
{
    [Fact]
    public void NewId_ReturnsUniqueGuid()
    {
        Guid id1 = GuidExtensions.NewId;
        Guid id2 = GuidExtensions.NewId;
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyGuid()
    {
        Guid value = Guid.Empty;
        var result = value.IsEmpty();
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyGuid()
    {
        var value = Guid.NewGuid();
        var result = value.IsEmpty();
        Assert.False(result);
    }

    [Fact]
    public void NotEmpty_ReturnsTrueForNonEmptyGuid()
    {
        var value = Guid.NewGuid();
        var result = value.IsNotEmpty();
        Assert.True(result);
    }

    [Fact]
    public void NotEmpty_ReturnsFalseForEmptyGuid()
    {
        Guid value = Guid.Empty;
        var result = value.IsNotEmpty();
        Assert.False(result);
    }

    [Fact]
    public void IsNull_ReturnsTrueForNullGuid()
    {
        Guid? value = null;
        var result = value.IsNull();
        Assert.True(result);
    }

    [Fact]
    public void IsNull_ReturnsFalseForNonNullGuid()
    {
        Guid? value = Guid.NewGuid();
        var result = value.IsNull();
        Assert.False(result);
    }

    [Fact]
    public void NotNull_ReturnsTrueForNonNullGuid()
    {
        Guid? value = Guid.NewGuid();
        var result = value.IsNotNull();
        Assert.True(result);
    }

    [Fact]
    public void NotNull_ReturnsFalseForNullGuid()
    {
        Guid? value = null;
        var result = value.IsNotNull();
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsTrueForNullGuid()
    {
        Guid? value = null;
        var result = value.IsNullOrEmpty();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsTrueForEmptyGuid()
    {
        Guid? value = Guid.Empty;
        var result = value.IsNullOrEmpty();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsFalseForNonNullNonEmptyGuid()
    {
        Guid? value = Guid.NewGuid();
        var result = value.IsNullOrEmpty();
        Assert.False(result);
    }

    [Fact]
    public void NotNullAndEmpty_ReturnsTrueForNonNullNonEmptyGuid()
    {
        Guid? value = Guid.NewGuid();
        var result = value.IsNotNullAndEmpty();
        Assert.True(result);
    }

    [Fact]
    public void NotNullAndEmpty_ReturnsFalseForNullGuid()
    {
        Guid? value = null;
        var result = value.IsNotNullAndEmpty();
        Assert.False(result);
    }

    [Fact]
    public void NotNullAndEmpty_ReturnsFalseForEmptyGuid()
    {
        Guid? value = Guid.Empty;
        var result = value.IsNotNullAndEmpty();
        Assert.False(result);
    }

    [Fact]
    public void ToInt64_ReturnsCorrectInt64Value()
    {
        var value = Guid.NewGuid();
        var result = value.ToInt64();
        Assert.Equal(BitConverter.ToInt64(value.ToByteArray(), 0), result);
    }

    [Fact]
    public void ToInt32_ReturnsCorrectInt32Value()
    {
        var value = Guid.NewGuid();
        var result = value.ToInt32();
        Assert.Equal(BitConverter.ToInt32(value.ToByteArray(), 0), result);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void GetTimestamp_ReturnsCorrectTimestampForV7Guid()
    {
        string guidString = "0193f884-a1b4-773f-b763-4b55cf0b6844";
        var guid = new Guid(guidString);

        var expectedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(0x0193F884A1B4);
        var actualTimestamp = guid.GetTimestamp();

        Assert.Equal(expectedTimestamp, actualTimestamp);
    }

    [Fact]
    public void GetTimestamp_ThrowsNotSupportedExceptionForNonV7Guid()
    {
        var nonV7Guid = Guid.NewGuid();

        var exception = Assert.Throws<NotSupportedException>(
            () => nonV7Guid.GetTimestamp()
        );

        Assert.Equal("This method is only supported for GUIDs with version 7.", exception.Message);
    }
#endif
}