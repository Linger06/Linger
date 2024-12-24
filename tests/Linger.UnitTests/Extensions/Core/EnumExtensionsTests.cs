using System.ComponentModel;

#if NET5_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using Linger;
using Linger.UnitTests;
using Linger.UnitTests.Extensions.Core;

#endif

namespace Linger.UnitTests.Extensions.Core;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [Description("Description for ValueOne")]
#if NET5_0_OR_GREATER
        [Display(Name = "Display for ValueOne")]
#endif
        ValueOne = 1,

        ValueTwo = 2
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
        Assert.Throws<ArgumentException>(() => itemName.GetEnum<TestEnum>());
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

    [Fact]
    public void GetDescription_ReturnsCorrectDescription()
    {
        TestEnum item = TestEnum.ValueOne;
        var result = item.GetDescription();
        Assert.Equal("Description for ValueOne", result);
    }

    [Fact]
    public void GetDescription_ReturnsEnumNameIfNoDescription()
    {
        TestEnum item = TestEnum.ValueTwo;
        var result = item.GetDescription();
        Assert.Equal("ValueTwo", result);
    }

#if NET5_0_OR_GREATER

    [Fact]
    public void GetDisplay_ReturnsCorrectDisplay()
    {
        TestEnum item = TestEnum.ValueOne;
        var result = item.GetDisplay();
        Assert.Equal("Display for ValueOne", result);
    }

    [Fact]
    public void GetDisplay_ReturnsEnumNameIfNoDisplay()
    {
        TestEnum item = TestEnum.ValueTwo;
        var result = item.GetDisplay();
        Assert.Equal("ValueTwo", result);
    }

#endif
}