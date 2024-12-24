namespace Linger.UnitTests.Extensions.Collection;

public class ICollectionExtensionsTests
{
    [Fact]
    public void RemoveAll_RemovesMatchingElements_FromList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        Func<int, bool> predicate = x => x % 2 == 0; // Remove even numbers

        // Act
        list.RemoveAll(predicate);

        // Assert
        Assert.Equal(new List<int> { 1, 3, 5 }, list);
    }

    [Fact]
    public void RemoveAll_RemovesMatchingElements_FromHashSet()
    {
        // Arrange
        var set = new HashSet<int> { 1, 2, 3, 4, 5 };
        Func<int, bool> predicate = x => x % 2 == 0; // Remove even numbers

        // Act
        set.RemoveAll(predicate);

        // Assert
        Assert.Equal(new HashSet<int> { 1, 3, 5 }, set);
    }

    [Fact]
    public void RemoveAll_DoesNotRemoveAnyElements_WhenNoMatch()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        Func<int, bool> predicate = x => x > 5; // No elements greater than 5

        // Act
        list.RemoveAll(predicate);

        // Assert
        Assert.Equal(new List<int> { 1, 2, 3, 4, 5 }, list);
    }

    [Fact]
    public void RemoveAll_RemovesAllElements_WhenAllMatch()
    {
        // Arrange
        var list = new List<int> { 2, 4, 6, 8 };
        Func<int, bool> predicate = x => x % 2 == 0; // Remove all even numbers

        // Act
        list.RemoveAll(predicate);

        // Assert
        Assert.Empty(list);
    }
}