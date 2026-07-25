using Xunit.v3;

namespace Linger.UnitTests;

public class SortInfoTests
{
    [Fact]
    public void SortInfo_PropertyShouldBeSettable()
    {
        var sortInfo = new SortInfo { Property = "InitialProperty" };
        var propertyName = "TestProperty";

        sortInfo.Property = propertyName;

        Assert.Equal(propertyName, sortInfo.Property);
    }

    [Fact]
    public void SortInfo_DirectionShouldBeSettable()
    {
        var sortInfo = new SortInfo { Property = "Name" };

        sortInfo.Direction = SortDir.Desc;

        Assert.Equal(SortDir.Desc, sortInfo.Direction);
    }

    [Fact]
    public void SortInfo_DefaultDirectionShouldBeAsc()
    {
        var sortInfo = new SortInfo { Property = "Name" };

        Assert.Equal(SortDir.Asc, sortInfo.Direction);
    }

    [Fact]
    public void SortInfo_ShouldBeInitializableWithProperties()
    {
        var sortInfo = new SortInfo
        {
            Property = "Name",
            Direction = SortDir.Desc
        };

        Assert.Equal("Name", sortInfo.Property);
        Assert.Equal(SortDir.Desc, sortInfo.Direction);
    }
}
