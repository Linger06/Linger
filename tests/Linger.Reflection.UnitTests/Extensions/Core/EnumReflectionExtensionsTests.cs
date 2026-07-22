using System.ComponentModel;
#if NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif
using Linger.Extensions.Core;

namespace Linger.UnitTests.Extensions.Core;

public class EnumReflectionExtensionsTests
{
    [Fact]
    public void GetDescription_WhenDescriptionExists_ReturnsDescription()
    {
        string result = TestEnum.Described.GetDescription();

        Assert.Equal("Description for Described", result);
    }

    [Fact]
    public void GetDescription_WhenDescriptionIsAbsent_ReturnsEnumName()
    {
        string result = TestEnum.Plain.GetDescription();

        Assert.Equal(nameof(TestEnum.Plain), result);
    }

#if NET5_0_OR_GREATER
    [Fact]
    public void GetDisplay_WhenDisplayExists_ReturnsDisplayName()
    {
        string result = TestEnum.Described.GetDisplay();

        Assert.Equal("Display for Described", result);
    }

    [Fact]
    public void GetDisplay_WhenDisplayIsAbsent_ReturnsEnumName()
    {
        string result = TestEnum.Plain.GetDisplay();

        Assert.Equal(nameof(TestEnum.Plain), result);
    }
#endif

    private enum TestEnum
    {
        [Description("Description for Described")]
#if NET5_0_OR_GREATER
        [Display(Name = "Display for Described")]
#endif
        Described,
        Plain
    }
}
