using Xunit.v3;

namespace Linger.UnitTests;

public class SortInfoTests
{
    [Fact]
    public void SortInfo_PropertyShouldBeSettable()
    {
        // Arrange
        var sortInfo = new SortInfo();
        var propertyName = "TestProperty";
        
        // Act
        sortInfo.Property = propertyName;
        
        // Assert
        Assert.Equal(propertyName, sortInfo.Property);
    }
    
    [Fact]
    public void SortInfo_DirectionShouldBeSettable()
    {
        // Arrange
        var sortInfo = new SortInfo();
        
        // Act
        sortInfo.Direction = SortDir.Desc;
        
        // Assert
        Assert.Equal(SortDir.Desc, sortInfo.Direction);
    }
    
    [Fact]
    public void SortInfo_DefaultDirectionShouldBeAsc()
    {
        // Arrange & Act
        var sortInfo = new SortInfo();
        
        // Assert
        Assert.Equal(SortDir.Asc, sortInfo.Direction);
    }
    
    [Fact]
    public void SortInfo_ShouldBeInitializableWithProperties()
    {
        // Arrange & Act
        var sortInfo = new SortInfo
        {
            Property = "Name",
            Direction = SortDir.Desc
        };
        
        // Assert
        Assert.Equal("Name", sortInfo.Property);
        Assert.Equal(SortDir.Desc, sortInfo.Direction);
    }
}