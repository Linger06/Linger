
#if NET5_0_OR_GREATER

using Linger;
using Linger.UnitTests;
using Linger.UnitTests.Extensions.Core;

#endif

namespace Linger.UnitTests.Extensions.Core;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        ValueOne = 1,

        ValueTwo = 2
    }

    [Fact]
    [Trait("GetEnum", "itemName")]
    public void GetEnum_EnumName_ReturnCorrespondEnum()
    {
        //Arrange
        StatusCode statusCode = StatusCode.Deleted;

        //Act
        var actual = statusCode.ToString();

        //Assert
        Assert.Equal(statusCode, actual.GetEnum<StatusCode>());
    }

    [Fact]
    [Trait("GetEnum", "itemValue")]
    public void GetEnum_EnumValue_ReturnCorrespondEnum()
    {
        //Arrange
        StatusCode statusCode = StatusCode.Disable;

        //Act
        var actual = statusCode.GetHashCode();

        //Assert
        Assert.Equal(statusCode, actual.GetEnum<StatusCode>());
    }

    [Fact]
    [Trait("GetEnumName", "itemValue")]
    public void GetEnumName_EnumValue_ReturnCorrespondEnumName()
    {
        //Arrange
        StatusCode statusCode = StatusCode.Enable;

        //Act
        var actual = statusCode.GetHashCode();

        //Assert
        Assert.Equal(statusCode.ToString(), actual.GetEnumName<StatusCode>());
    }

    [Fact]
    public void GetEnum_ReturnsCorrectEnumForString()
    {
        var itemName = "ValueOne";
        TestEnum result = itemName.GetEnum<TestEnum>();
        Assert.Equal(TestEnum.ValueOne, result);
    }

    [Fact]
    public void GetEnum_ThrowsExceptionForInvalidString()
    {
        var itemName = "InvalidValue";
        Assert.Throws<System.ArgumentException>(() => itemName.GetEnum<TestEnum>());
    }

    [Fact]
    public void GetEnum_ThrowsExceptionForUndefinedNumericString()
    {
        Assert.Throws<ArgumentException>(() => "999".GetEnum<TestEnum>());
    }

    [Fact]
    public void TryGetEnum_FailsForUndefinedNumericString()
    {
        var ok = "999".TryGetEnum<TestEnum>(out var value);

        Assert.False(ok);
        Assert.Equal(default, value);
    }

    [Fact]
    public void ToEnum_ReturnsCorrectEnumForString()
    {
        var itemName = "ValueTwo";
        TestEnum result = itemName.ToEnum<TestEnum>();
        Assert.Equal(TestEnum.ValueTwo, result);
    }

    [Fact]
    public void GetEnum_ReturnsCorrectEnumForInt()
    {
        var itemValue = 1;
        TestEnum result = itemValue.GetEnum<TestEnum>();
        Assert.Equal(TestEnum.ValueOne, result);
    }

    [Fact]
    public void GetEnum_ThrowsExceptionForInvalidInt()
    {
        var itemValue = 99;
        Assert.Throws<InvalidOperationException>(() => itemValue.GetEnum<TestEnum>());
    }

    [Fact]
    public void TryGetEnum_String_Succeeds_ForValidName()
    {
        var ok = "ValueOne".TryGetEnum<TestEnum>(out var value);
        Assert.True(ok);
        Assert.Equal(TestEnum.ValueOne, value);
    }

    [Fact]
    public void TryGetEnum_String_Fails_ForInvalidName()
    {
        var ok = "NoSuch".TryGetEnum<TestEnum>(out var value);
        Assert.False(ok);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetEnum_Int_Succeeds_ForDefinedValue()
    {
        var ok = 2.TryGetEnum<TestEnum>(out var value);
        Assert.True(ok);
        Assert.Equal(TestEnum.ValueTwo, value);
    }

    [Fact]
    public void TryGetEnum_Int_Fails_ForUndefinedValue()
    {
        var ok = 123.TryGetEnum<TestEnum>(out var value);
        Assert.False(ok);
        Assert.Equal(default, value);
    }

    [Fact]
    public void GetEnumName_ReturnsCorrectNameForInt()
    {
        var itemValue = 1;
        var result = itemValue.GetEnumName<TestEnum>();
        Assert.Equal("ValueOne", result);
    }

    [Fact]
    public void GetEnumName_ReturnsNullForInvalidInt()
    {
        var itemValue = 99;
        var result = itemValue.GetEnumName<TestEnum>();
        Assert.Null(result);
    }

}
