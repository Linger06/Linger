using Linger;
using Linger.Enums;
using Xunit.v3;

namespace Linger.UnitTests.Enums;

public class EnumExtensionsGeneratorTests
{
    [Fact]
    public void SortDir_String_TryGet_ShouldParseName_IgnoringCase()
    {
        var ok = "asc".TryGet(out SortDir dir);
        Assert.True(ok);
        Assert.Equal(SortDir.Asc, dir);

        ok = "DESC".TryGet(out dir);
        Assert.True(ok);
        Assert.Equal(SortDir.Desc, dir);
    }

    [Fact]
    public void SortDir_Int_TryGet_And_FromInt_ShouldWork()
    {
        int zero = 0;
        Assert.True(zero.TryGet(out SortDir dir0));
        Assert.Equal(SortDir.Asc, dir0);

        int one = 1;
        Assert.True(one.TryGet(out SortDir dir1));
        Assert.Equal(SortDir.Desc, dir1);

        var v = SortDirExtensions.FromInt(1);
        Assert.Equal(SortDir.Desc, v);
    }

    [Fact]
    public void SortDir_Int_TryGet_Invalid_ShouldReturnFalse()
    {
        int invalid = 2;
        Assert.False(invalid.TryGet(out SortDir _));
    }

    [Fact]
    public void SortDir_GetName_And_DisplayName_ShouldBeMemberName_WhenNoDescription()
    {
        Assert.Equal("Asc", SortDir.Asc.GetName());
        Assert.Equal("Asc", SortDir.Asc.GetDisplayName());
        Assert.Equal("Desc", SortDir.Desc.GetName());
        Assert.Equal("Desc", SortDir.Desc.GetDisplayName());
    }

    [Fact]
    public void CompareOperator_TryGet_ByName_IgnoringCase()
    {
        var ok = "contains".TryGet(out CompareOperator op);
        Assert.True(ok);
        Assert.Equal(CompareOperator.Contains, op);

        ok = "NOTCONTAINS".TryGet(out op);
        Assert.True(ok);
        Assert.Equal(CompareOperator.NotContains, op);
    }

    [Fact]
    public void CompareOperator_DisplayName_ShouldPreferDescriptionOrFallbackToName()
    {
        // All members currently have [Description], equal to name
        Assert.Equal("Equals", CompareOperator.Equals.GetDisplayName());
        Assert.Equal("GreaterThan", CompareOperator.GreaterThan.GetDisplayName());
        // Smoke check map size > 0
        var map = CompareOperatorExtensions.GetDisplayMap();
        Assert.True(map.Count >= 1);
    }
}
