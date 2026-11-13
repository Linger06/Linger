namespace Linger.UnitTests.Extensions.Collection;

public class ICollectionExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemoveAll_WithEqualItems_RemovesOnlyMatchingInstance(bool linkedList)
    {
        var retained = new EqualItem { Id = 1, Remove = false };
        var removed = new EqualItem { Id = 1, Remove = true };
        ICollection<EqualItem> collection = linkedList
            ? new LinkedList<EqualItem>(new[] { retained, removed })
            : new System.Collections.ObjectModel.ObservableCollection<EqualItem>(new[] { retained, removed });

        collection.RemoveAll(item => item.Remove);

        Assert.Same(retained, Assert.Single(collection));
    }

    private sealed class EqualItem
    {
        public int Id { get; set; }
        public bool Remove { get; set; }

        public override bool Equals(object? obj) => obj is EqualItem other && Id == other.Id;

        public override int GetHashCode() => Id;
    }

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

    [Fact]
    public void RemoveAll_WithNullCollection_ThrowsArgumentNullException()
    {
        ICollection<int>? collection = null;

        Assert.Throws<ArgumentNullException>(() => collection!.RemoveAll(_ => true));
    }

    [Fact]
    public void RemoveAll_WithNullPredicate_ThrowsArgumentNullException()
    {
        ICollection<int> collection = new List<int>();

        Assert.Throws<ArgumentNullException>(() => collection.RemoveAll(null!));
    }
}
