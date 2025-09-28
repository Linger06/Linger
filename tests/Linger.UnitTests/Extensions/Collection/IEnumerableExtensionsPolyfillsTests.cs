using System;
using Xunit;

namespace Linger.UnitTests.Extensions.Collection;

public class IEnumerableExtensionsPolyfillsTests
{
    private readonly List<Person> _people;
    private readonly List<Order> _orders;

    public IEnumerableExtensionsPolyfillsTests()
    {
        _people = new List<Person>
        {
            new() { Id = 1, Name = "John" },
            new() { Id = 2, Name = "Jane" },
            new() { Id = 3, Name = "Bob" }
        };

        _orders = new List<Order>
        {
            new() { Id = 1, PersonId = 1, Product = "Laptop" },
            new() { Id = 2, PersonId = 1, Product = "Mouse" },
            new() { Id = 3, PersonId = 2, Product = "Keyboard" }
            // Note: No order for Bob (PersonId = 3)
        };
    }

#if !NET6_0_OR_GREATER
    [Fact]
    public void DistinctBy_ReturnsDistinctElementsByKey()
    {
        // Arrange
        var items = new[]
        {
            new { Id = 1, Name = "John" },
            new { Id = 2, Name = "Jane" },
            new { Id = 3, Name = "John" }, // Duplicate name
            new { Id = 4, Name = "Bob" }
        };

        // Act
        var result = items.DistinctBy(x => x.Name).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Name == "John");
        Assert.Contains(result, x => x.Name == "Jane");
        Assert.Contains(result, x => x.Name == "Bob");
        Assert.Single(result.Where(x => x.Name == "John"));
    }

    [Fact]
    public void DistinctBy_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        // Arrange
        IEnumerable<string>? source = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => source!.DistinctBy(x => x.Length).ToList());
    }

    [Fact]
    public void DistinctBy_ThrowsArgumentNullException_WhenKeySelectorIsNull()
    {
        // Arrange
        var source = new[] { "a", "b", "c" };

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => source.DistinctBy<string, int>(null!).ToList());
    }
#endif

#if !NET10_0_OR_GREATER
    [Fact]
    public void LeftJoin_WithResultSelector_ReturnsCorrectResults()
    {
        // Act
        var result = _people.LeftJoin(
            _orders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person.Name, Product = order?.Product ?? "No Order" }
        ).ToList();

        // Assert
        Assert.Equal(4, result.Count); // John has 2 orders, Jane has 1, Bob has 0 = 4 total
        
        // Check John's orders
        var johnOrders = result.Where(r => r.Person == "John").ToList();
        Assert.Equal(2, johnOrders.Count);
        Assert.Contains(johnOrders, r => r.Product == "Laptop");
        Assert.Contains(johnOrders, r => r.Product == "Mouse");
        
        // Check Jane's order
        var janeOrders = result.Where(r => r.Person == "Jane").ToList();
        Assert.Single(janeOrders);
        Assert.Equal("Keyboard", janeOrders[0].Product);
        
        // Check Bob (no orders)
        var bobOrders = result.Where(r => r.Person == "Bob").ToList();
        Assert.Single(bobOrders);
        Assert.Equal("No Order", bobOrders[0].Product);
    }

    [Fact]
    public void LeftJoin_WithCustomComparer_ReturnsCorrectResults()
    {
        // Arrange
        var stringPeople = new[] { new { Id = "1", Name = "John" }, new { Id = "2", Name = "Jane" } };
        var stringOrders = new[] { new { Id = 1, PersonId = "1", Product = "Laptop" } };

        // Act
        var result = stringPeople.LeftJoin(
            stringOrders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person.Name, Product = order?.Product ?? "No Order" },
            StringComparer.OrdinalIgnoreCase
        ).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("John", result[0].Person);
        Assert.Equal("Laptop", result[0].Product);
        Assert.Equal("Jane", result[1].Person);
        Assert.Equal("No Order", result[1].Product);
    }

    [Fact]
    public void LeftJoin_ReturnsCorrectTuples()
    {
        // Act
        var result = _people.LeftJoin(
            _orders,
            p => p.Id,
            o => o.PersonId
        ).ToList();

        // Assert
        Assert.Equal(4, result.Count); // John has 2 orders, Jane has 1, Bob has 0 = 4 total
        
        // Check John's results
        var johnResults = result.Where(r => r.Item1.Name == "John").ToList();
        Assert.Equal(2, johnResults.Count);
        Assert.Contains(johnResults, r => r.Item2?.Product == "Laptop");
        Assert.Contains(johnResults, r => r.Item2?.Product == "Mouse");
        
        // Check Bob (no orders)
        var bobResults = result.Where(r => r.Item1.Name == "Bob").ToList();
        Assert.Single(bobResults);
        Assert.Null(bobResults[0].Item2);
    }

    [Fact]
    public void LeftJoin_ThrowsArgumentNullException_WhenParametersAreNull()
    {
        // Assert
        Assert.Throws<System.ArgumentNullException>(() => ((IEnumerable<Person>)null!).LeftJoin(_orders, p => p.Id, o => o.PersonId, (p, o) => new { p, o }).ToList());
        
        Assert.Throws<System.ArgumentNullException>(() => _people.LeftJoin((IEnumerable<Order>)null!, p => p.Id, o => o.PersonId, (p, o) => new { p, o }).ToList());
        
        Assert.Throws<System.ArgumentNullException>(() => _people.LeftJoin(_orders, null!, o => o.PersonId, (p, o) => new { p, o }).ToList());
        
        Assert.Throws<System.ArgumentNullException>(() => _people.LeftJoin(_orders, p => p.Id, null!, (p, o) => new { p, o }).ToList());
        
        Assert.Throws<System.ArgumentNullException>(() => _people.LeftJoin(_orders, p => p.Id, o => o.PersonId, (Func<Person, Order?, object>)null!).ToList());
    }

    [Fact]
    public void RightJoin_WithResultSelector_ReturnsCorrectResults()
    {
        // Act
        var result = _people.RightJoin(
            _orders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person?.Name ?? "Unknown", Product = order.Product }
        ).ToList();

        // Assert
        Assert.Equal(3, result.Count); // All orders should be included
        Assert.Equal("John", result[0].Person);
        Assert.Equal("Laptop", result[0].Product);
        Assert.Equal("John", result[1].Person);
        Assert.Equal("Mouse", result[1].Product);
        Assert.Equal("Jane", result[2].Person);
        Assert.Equal("Keyboard", result[2].Product);
    }

    [Fact]
    public void RightJoin_WithCustomComparer_ReturnsCorrectResults()
    {
        // Arrange
        var stringPeople = new[] { new { Id = "1", Name = "John" } };
        var stringOrders = new[] 
        { 
            new { Id = 1, PersonId = "1", Product = "Laptop" },
            new { Id = 2, PersonId = "2", Product = "Mouse" } // No matching person
        };

        // Act
        var result = stringPeople.RightJoin(
            stringOrders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person?.Name ?? "Unknown", Product = order.Product },
            StringComparer.OrdinalIgnoreCase
        ).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("John", result[0].Person);
        Assert.Equal("Laptop", result[0].Product);
        Assert.Equal("Unknown", result[1].Person);
        Assert.Equal("Mouse", result[1].Product);
    }

    [Fact]
    public void RightJoin_WithEmptyLeftSequence_ReturnsAllRightItems()
    {
        // Arrange
        var emptyPeople = new List<Person>();

        // Act
        var result = emptyPeople.RightJoin(
            _orders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person?.Name ?? "Unknown", Product = order.Product }
        ).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.Equal("Unknown", r.Person));
    }

    [Fact]
    public void RightJoin_WithEmptyRightSequence_ReturnsEmpty()
    {
        // Arrange
        var emptyOrders = new List<Order>();

        // Act
        var result = _people.RightJoin(
            emptyOrders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person?.Name ?? "Unknown", Product = order.Product }
        ).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void LeftJoin_MultipleMatches_ReturnsAllCombinations()
    {
        // Act - John has 2 orders, so should appear twice
        var result = _people.LeftJoin(
            _orders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person.Name, Product = order?.Product ?? "No Order" }
        ).Where(x => x.Person == "John").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Product == "Laptop");
        Assert.Contains(result, r => r.Product == "Mouse");
    }

    [Fact]
    public void RightJoin_PerformanceWithLargeDataSet()
    {
        // Arrange
        var largePeople = Enumerable.Range(1, 1000).Select(i => new Person { Id = i, Name = $"Person{i}" }).ToList();
        var largeOrders = Enumerable.Range(1, 500).Select(i => new Order { Id = i, PersonId = i * 2, Product = $"Product{i}" }).ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = largePeople.RightJoin(
            largeOrders,
            p => p.Id,
            o => o.PersonId,
            (person, order) => new { Person = person?.Name, Product = order.Product }
        ).ToList();
        stopwatch.Stop();

        // Assert
        Assert.Equal(500, result.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, "RightJoin should complete within reasonable time");
    }
#endif

    private class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Order
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string Product { get; set; } = string.Empty;
    }
}
