namespace Linger.Extensions.Tests;

public partial class ArrayExtensionsTests
{
    [Fact]
    public void ForEach_Action_ShouldApplyActionToEachElement()
    {
        int[] array = [1, 2, 3];
        var sum = 0;
        array.ForEach(x => sum += x);
        Assert.Equal(6, sum);
    }

    [Fact]
    public void ForEach_ActionWithIndex_ShouldApplyActionToEachElementWithIndex()
    {
        int[] array = [1, 2, 3];
        var sum = 0;
        array.ForEach((x, i) => sum += x * i);
        Assert.Equal(8, sum);
    }

    [Fact]
    public void Exists_ShouldReturnTrueIfElementExists()
    {
        int[] array = [1, 2, 3];
        var exists = array.Exists(2);
        Assert.True(exists);
    }

    [Fact]
    public void Exists_ShouldReturnFalseIfElementDoesNotExist()
    {
        int[] array = [1, 2, 3];
        var exists = array.Exists(4);
        Assert.False(exists);
    }

    [Fact]
    public void Exists_WithPredicate_ShouldReturnTrueIfElementMatchesPredicate()
    {
        int[] array = [1, 2, 3];
        var exists = array.Exists(x => x > 2);
        Assert.True(exists);
    }

    [Fact]
    public void Insert_ShouldInsertElementAtBeginning()
    {
        int[] array = [1, 2, 3];
        var result = array.Insert(0);
        Assert.Equal([1, 2, 3, 0], result);
    }

    [Fact]
    public void Add_ShouldAddElementAtEnd()
    {
        int[] array = [1, 2, 3];
        var result = array.Add(4);
        Assert.Equal([1, 2, 3, 4], result);
    }

    [Fact]
    public void Remove_ShouldRemoveSpecifiedElement()
    {
        int[] array = [1, 2, 3, 2];
        var result = array.Remove(2);
        Assert.Equal([1, 3], result);
    }

    [Fact]
    public void RemoveAt_ShouldRemoveElementAtSpecifiedIndex()
    {
        int[] array = [1, 2, 3];
        var result = array.RemoveAt(1);
        Assert.Equal([1, 3], result);
    }

    [Fact]
    public void RemoveRange_ShouldRemoveElementsAfterSpecifiedIndex()
    {
        int[] array = [1, 2, 3, 4, 5];
        var result = array.RemoveRange(2);
        Assert.Equal([1, 2], result);
    }

    [Fact]
    public void RemoveRange_WithLength_ShouldRemoveSpecifiedRangeOfElements()
    {
        int[] array = [1, 2, 3, 4, 5];
        var result = array.RemoveRange(1, 3);
        Assert.Equal([1, 5], result);
    }
}