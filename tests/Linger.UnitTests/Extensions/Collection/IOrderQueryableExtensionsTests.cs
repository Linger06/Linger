#if NET5_0_OR_GREATER

namespace Linger.UnitTests;

public class IOrderQueryableExtensionsTests
{
    [Fact]
    public void ThenBy_SortsBySinglePropertyAscending()
    {
        IOrderedQueryable<SampleClass>? list = new List<SampleClass>
    {
        new SampleClass { Id = 2, Name = "B" },
        new SampleClass { Id = 1, Name = "A" },
        new SampleClass { Id = 1, Name = "C" }
    }.AsQueryable().OrderBy(x => x.Id);

        var result = list.ThenBy("Name", true).ToList();

        Assert.Equal("A", result[0].Name);
        Assert.Equal("C", result[1].Name);
    }

    [Fact]
    public void ThenBy_SortsBySinglePropertyDescending()
    {
        IOrderedQueryable<SampleClass>? list = new List<SampleClass>
    {
        new SampleClass { Id = 1, Name = "A" },
        new SampleClass { Id = 2, Name = "B" },
        new SampleClass { Id = 1, Name = "C" }
    }.AsQueryable().OrderBy(x => x.Id);

        var result = list.ThenBy("Name", false).ToList();

        Assert.Equal("C", result[0].Name);
        Assert.Equal("A", result[1].Name);
    }

    [Fact]
    public void ThenBy_SortsByMultipleProperties()
    {
        IOrderedQueryable<SampleClass>? list = new List<SampleClass>
    {
        new SampleClass { Id = 1, Name = "B" },
        new SampleClass { Id = 1, Name = "A" },
        new SampleClass { Id = 2, Name = "C" }
    }.AsQueryable().OrderBy(x => x.Id);

        var result = list.ThenBy(new KeyValuePair<string, bool>[]
        {
        new KeyValuePair<string, bool>("Id", true),
        new KeyValuePair<string, bool>("Name", true)
        }).ToList();

        Assert.Equal("A", result[0].Name);
        Assert.Equal("B", result[1].Name);
        Assert.Equal("C", result[2].Name);
    }

    [Fact]
    public void ThenBy_HandlesEmptyOrderByPropertyList()
    {
        IOrderedQueryable<SampleClass>? list = new List<SampleClass>
    {
        new SampleClass { Id = 1, Name = "A" },
        new SampleClass { Id = 2, Name = "B" }
    }.AsQueryable().OrderBy(x => x.Id);

        var result = list.ThenBy(new KeyValuePair<string, bool>[0]).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
        Assert.Equal("B", result[1].Name);
    }

    [Fact]
    public void ThenBy_ThrowsArgumentExceptionForInvalidPropertyName()
    {
        IOrderedQueryable<SampleClass>? list = new List<SampleClass>
    {
        new SampleClass { Id = 1, Name = "A" },
        new SampleClass { Id = 2, Name = "B" }
    }.AsQueryable().OrderBy(x => x.Id);

        Assert.Throws<System.ArgumentException>(() => list.ThenBy("InvalidProperty", true).ToList());
    }

    private class SampleClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}

#endif
