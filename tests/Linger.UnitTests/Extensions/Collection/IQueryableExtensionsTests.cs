namespace Linger.UnitTests.Extensions.Collection;

public class IQueryableExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [Fact]
    public void CreateOrderBy_ShouldOrderByAscending()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        // Act
        var result = data.CreateOrderBy("Id", true).ToList();

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
    }

    [Fact]
    public void CreateOrderBy_ShouldOrderByDescending()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "B" }
        }.AsQueryable();

        // Act
        var result = data.CreateOrderBy("Id", false).ToList();

        // Assert
        Assert.Equal(2, result[0].Id);
        Assert.Equal(1, result[1].Id);
    }

    [Fact]
    public void CreateOrderBy_ShouldThrowArgumentException_WhenPropertyNotFound()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>().AsQueryable();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.CreateOrderBy("NonExistentProperty"));
    }

    [Fact]
    public void OrderByIf_ShouldOrderBy_WhenConditionIsTrue()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        // Act
        var result = data.OrderByIf<TestEntity, IQueryable<TestEntity>>(true, "Id").ToList();

        // Assert
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
    }

    [Fact]
    public void OrderByIf_ShouldNotOrderBy_WhenConditionIsFalse()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        // Act
        var result = data.OrderByIf<TestEntity, IQueryable<TestEntity>>(false, "Id").ToList();

        // Assert
        Assert.Equal(2, result[0].Id);
        Assert.Equal(1, result[1].Id);
    }

#if NET5_0_OR_GREATER

    [Fact]
    public void CreateOrderBy_WithSortList_ShouldOrderByMultipleProperties()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 1, Name = "C" }
        }.AsQueryable();

        var sortList = new List<SortInfo>
        {
            new SortInfo { Property = "Id", Direction = SortDir.Asc },
            new SortInfo { Property = "Name", Direction = SortDir.Desc }
        };

        // Act
        var result = data.CreateOrderBy(sortList).ToList();

        // Assert
        Assert.Equal("C", result[0].Name);
        Assert.Equal("A", result[1].Name);
        Assert.Equal("B", result[2].Name);
    }

    [Fact]
    public void CreateOrderBy_WithEmptySortList_ShouldReturnOriginalQuery()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        var sortList = new List<SortInfo>();

        // Act
        var result = data.CreateOrderBy(sortList).ToList();

        // Assert
        Assert.Equal(2, result[0].Id);
        Assert.Equal(1, result[1].Id);
    }

    [Fact]
    public void CreateOrderBy_WithSingleProperty_ShouldOrderByCorrectly()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        KeyValuePair<string, bool>[]? orderByPropertyList = new[]
        {
            new KeyValuePair<string, bool>("Id", true)
        };

        // Act
        var result = data.CreateOrderBy(orderByPropertyList).ToList();

        // Assert
        Assert.Single(orderByPropertyList);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
    }

    [Fact]
    public void CreateOrderBy_WithMultipleProperties_ShouldOrderByCorrectly()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 1, Name = "C" }
        }.AsQueryable();

        KeyValuePair<string, bool>[]? orderByPropertyList = new[]
        {
            new KeyValuePair<string, bool>("Id", true),
            new KeyValuePair<string, bool>("Name", false)
        };

        // Act
        var result = data.CreateOrderBy(orderByPropertyList).ToList();

        // Assert
        Assert.Equal(2, orderByPropertyList.Length);
        Assert.Equal("C", result[0].Name);
        Assert.Equal("A", result[1].Name);
        Assert.Equal("B", result[2].Name);
    }

    [Fact]
    public void CreateOrderBy_WithZeroProperties_ShouldReturnOriginalQuery()
    {
        // Arrange
        IQueryable<TestEntity>? data = new List<TestEntity>
        {
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 1, Name = "A" }
        }.AsQueryable();

        KeyValuePair<string, bool>[]? orderByPropertyList = Array.Empty<KeyValuePair<string, bool>>();

        // Assert
        Assert.Throws<System.ArgumentException>(() => data.CreateOrderBy(orderByPropertyList));
    }

#endif
}
