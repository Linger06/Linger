namespace Linger.UnitTests.Extensions.Core;

public class EnumExtensionsTest
{
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
    [Trait("GetDescription", "Enum")]
    public void GetDescription_Enum_ReturnCorrespondEnumDescription()
    {
        //Arrange
        StatusCode statusCode = StatusCode.Deleted;

        //Assert
        Assert.Equal("Deleted", statusCode.GetDescription());
    }
}