namespace Linger.Extensions.Tests;

public partial class ArrayExtensionsTests
{
    [Fact]
    public void ForEach_ActionOnEachElement_ShouldExecuteAction()
    {
        int[] numbers = { 1, 2, 3 };
        var sum = 0;
        numbers.ForEach(n => sum += n);
        Assert.Equal(6, sum);
    }

    [Fact]
    public void ForEach_ActionWithIndex_ShouldExecuteActionWithIndex()
    {
        int[] numbers = { 1, 2, 3 };
        var result = "";
        numbers.ForEach((n, i) => result += $"{i}:{n} ");
        Assert.Equal("0:1 1:2 2:3 ", result);
    }

    [Fact]
    public void ForEach_EmptyArray_ShouldNotExecuteAction()
    {
        int[] numbers = { };
        var sum = 0;
        numbers.ForEach(n => sum += n);
        Assert.Equal(0, sum);
    }

    [Fact]
    public void ForEach_NullAction_ShouldThrowArgumentNullException()
    {
        int[] numbers = { 1, 2, 3 };
        Assert.Throws<ArgumentNullException>(() => numbers.ForEach((Action<int>)null));
    }

    [Fact]
    public void ForEach_WithIndex_NullAction_ShouldThrowArgumentNullException()
    {
        int[] numbers = { 1, 2, 3 };
        Assert.Throws<ArgumentNullException>(() => numbers.ForEach((Action<int, int>)null));
    }

    [Fact]
    public void Exists_ValueExists_ShouldReturnTrue()
    {
        int[] numbers = { 1, 2, 3 };
        var exists = numbers.Exists(2);
        Assert.True(exists);
    }

    [Fact]
    public void Exists_ValueDoesNotExist_ShouldReturnFalse()
    {
        int[] numbers = { 1, 2, 3 };
        var exists = numbers.Exists(4);
        Assert.False(exists);
    }

    [Fact]
    public void Exists_NullValueInArray_ShouldReturnTrue()
    {
        string[] strings = { "a", null, "c" };
        var exists = strings.Exists(value: null);
        Assert.True(exists);
    }

    [Fact]
    public void Exists_NullValueNotInArray_ShouldReturnFalse()
    {
        string[] strings = { "a", "b", "c" };
        var exists = strings.Exists(value: null);
        Assert.False(exists);
    }

    [Fact]
    public void Exists_ValueInArrayWithNull_ShouldReturnTrue()
    {
        string?[] strings = { "a", null, "c" };
        var exists = strings.Exists(value: "c");
        Assert.True(exists);
    }

    [Fact]
    public void Exists_EmptyArray_ShouldReturnFalse()
    {
        int[] numbers = { };
        var exists = numbers.Exists(1);
        Assert.False(exists);
    }

    [Fact]
    public void Remove_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3, 2 };
        var newNumbers = numbers.Remove(2);
        Assert.Equal(new int[] { 1, 3 }, newNumbers);
    }

    [Fact]
    public void Remove_ValueNotInArray_ShouldReturnSameArray()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.Remove(4);
        Assert.Equal(new int[] { 1, 2, 3 }, newNumbers);
    }

    [Fact]
    public void Remove_NullValueInArray_ShouldReturnNewArray()
    {
        string[] strings = { "a", null, "c" };
        string[] newStrings = strings.Remove(null);
        Assert.Equal(new string[] { "a", "c" }, newStrings);
    }

    [Fact]
    public void Remove_NullValueNotInArray_ShouldReturnSameArray()
    {
        string[] strings = { "a", "b", "c" };
        string[] newStrings = strings.Remove(null);
        Assert.Equal(new string[] { "a", "b", "c" }, newStrings);
    }

    [Fact]
    public void Remove_ValueInArrayWithNull_ShouldReturnSameArray()
    {
        string[] strings = { "a", null, "c" };
        string[] newStrings = strings.Remove("a");
        Assert.Equal(new string[] { null, "c" }, newStrings);
    }

    [Fact]
    public void Remove_EmptyArray_ShouldReturnEmptyArray()
    {
        int[] numbers = { };
        var newNumbers = numbers.Remove(1);
        Assert.Empty(newNumbers);
    }

    [Fact]
    public void Insert_ValueAtEnd_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.Insert(4);
        Assert.Equal(new int[] { 1, 2, 3, 4 }, newNumbers);
    }

    [Fact]
    public void Add_ValueAtEnd_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.Add(4);
        Assert.Equal(new int[] { 1, 2, 3, 4 }, newNumbers);
    }

    [Fact]
    public void RemoveAt_ValidIndex_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.RemoveAt(1);
        Assert.Equal(new int[] { 1, 3 }, newNumbers);
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ShouldReturnEmptyArray()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.RemoveAt(3);
        Assert.Empty(newNumbers);
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ShouldReturnEmptyArray2()
    {
        int[] numbers = { 1, 2, 3 };
        var newNumbers = numbers.RemoveAt(-1);
        Assert.Empty(newNumbers);
    }

    [Fact]
    public void RemoveRange_FromStartIndex_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3, 4 };
        var newNumbers = numbers.RemoveRange(2);
        Assert.Equal(new int[] { 1, 2 }, newNumbers);
    }

    [Fact]
    public void RemoveRange_InvalidStartIndex_ShouldThrowException()
    {
        int[] numbers = { 1, 2, 3, 4 };
        Assert.Throws<IndexOutOfRangeException>(() => numbers.RemoveRange(5));
    }

    [Fact]
    public void RemoveRange_WithLength_ShouldReturnNewArray()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        var newNumbers = numbers.RemoveRange(1, 3);
        Assert.Equal(new int[] { 1, 5 }, newNumbers);
    }

    [Fact]
    public void RemoveRange_InvalidRange_ShouldThrowException()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        Assert.Throws<IndexOutOfRangeException>(() => numbers.RemoveRange(1, 5));
    }

    [Fact]
    public void RemoveRange_InvalidStartIndex_ShouldThrowException2()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        Assert.Throws<IndexOutOfRangeException>(() => numbers.RemoveRange(-1));
    }

    [Fact]
    public void RemoveRange_InvalidStartIndex_WithLength_ShouldThrowException()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        Assert.Throws<IndexOutOfRangeException>(() => numbers.RemoveRange(6, 5));
    }

    [Fact]
    public void RemoveRange_InvalidStartIndex_WithLength_ShouldThrowException2()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        Assert.Throws<IndexOutOfRangeException>(() => numbers.RemoveRange(-1, 5));
    }
}