namespace Linger.UnitTests.Extensions.Core;

public partial class ArrayExtensionsTests
{
    [Fact]
    public void Find_ShouldReturnFirstMatch()
    {
        int[] array = { 1, 2, 3, 4, 5 };
        var result = array.Find(x => x > 3);
        Assert.Equal(4, result);
    }

    [Fact]
    public void FindAll_ShouldReturnAllMatches()
    {
        int[] array = { 1, 2, 3, 4, 5 };
        var result = array.FindAll(x => x > 3);
        Assert.Equal(new int[] { 4, 5 }, result);
    }
}