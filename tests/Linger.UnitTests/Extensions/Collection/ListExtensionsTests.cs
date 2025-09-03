namespace Linger.UnitTests.Extensions.Collection;

public class ListExtensionsTests
{
    // String-specific Paging tests
    [Fact]
    public void Paging_ReturnsCorrectPage()
    {
        var list = new List<string> { "A", "B", "C", "D", "E" };

        var result = list.Paging(2, 2);

        Assert.Equal(new List<string> { "C", "D" }, result);
    }

    [Fact]
    public void Paging_ReturnsEmptyListForOutOfRangePage()
    {
        var list = new List<string> { "A", "B", "C" };

        var result = list.Paging(4, 2);

        Assert.Empty(result);
    }

    [Fact]
    public void Paging_ReturnsPartialPageForLastPage()
    {
        var list = new List<string> { "A", "B", "C", "D" };

        var result = list.Paging(2, 3);

        Assert.Equal(new List<string> { "D" }, result);
    }

    [Fact]
    public void Paging_ReturnsEmptyListForEmptyInputList()
    {
        var list = new List<string>();

        var result = list.Paging(1, 2);

        Assert.Empty(result);
    }

    // Generic Paging tests
    [Fact]
    public void PagingGeneric_ReturnsCorrectPageForIntegers()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var result = list.Paging(2, 2);

        Assert.Equal(new List<int> { 3, 4 }, result);
    }

    [Fact]
    public void PagingGeneric_ReturnsEmptyListForOutOfRangePage()
    {
        var list = new List<int> { 1, 2, 3 };

        var result = list.Paging(4, 2);

        Assert.Empty(result);
    }

    [Fact]
    public void PagingGeneric_ReturnsPartialPageForLastPage()
    {
        var list = new List<int> { 1, 2, 3, 4 };

        var result = list.Paging(2, 3);

        Assert.Equal(new List<int> { 4 }, result);
    }

    [Fact]
    public void PagingGeneric_ReturnsEmptyListForEmptyInputList()
    {
        var list = new List<int>();

        var result = list.Paging(1, 2);

        Assert.Empty(result);
    }

    [Fact]
    public void PagingGeneric_ReturnsEmptyListForNullInputList()
    {
        List<int>? list = null;

        var result = list.Paging(1, 2);

        Assert.Empty(result);
    }

    [Fact]
    public void PagingGeneric_WorksWithCustomObjects()
    {
        var list = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "First" },
            new SampleClass { Id = 2, Name = "Second" },
            new SampleClass { Id = 3, Name = "Third" },
            new SampleClass { Id = 4, Name = "Fourth" }
        };

        var result = list.Paging(2, 2).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
        Assert.Equal(4, result[1].Id);
    }

    [Fact]
    public void ToSeparatedString_FormatsCorrectlyWithDefaultParameters()
    {
        var list = new List<string> { "A", "B", "C" };

        var result = list.ToSeparatedString();

        Assert.Equal("'A','B','C'", result);
    }

    [Fact]
    public void ToSeparatedString_FormatsCorrectlyWithCustomSeparator()
    {
        var list = new List<string> { "A", "B", "C" };

        var result = list.ToSeparatedString(";");

        Assert.Equal("'A';'B';'C'", result);
    }

    [Fact]
    public void ToSeparatedString_FormatsCorrectlyWithoutSingleQuotes()
    {
        var list = new List<string> { "A", "B", "C" };

        var result = list.ToSeparatedString(",", false);

        Assert.Equal("A,B,C", result);
    }

    [Fact]
    public void ToSeparatedString_ReturnsEmptyStringForEmptyInputList()
    {
        var list = new List<string>();

        var result = list.ToSeparatedString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToTree_CreatesTreeStructureCorrectly()
    {
        var list = new List<SampleClass>
        {
            new SampleClass { Id = 1, ParentId = 0, Name = "Root" },
            new SampleClass { Id = 2, ParentId = 1, Name = "Child1" },
            new SampleClass { Id = 3, ParentId = 1, Name = "Child2" }
        };

        List<SampleClass>? result = list.ToTree(
            (_, child) => child.ParentId == 0,
            (parent, child) => parent.Id == child.ParentId,
            (parent, children) =>
            {
                parent.Children = parent.Children ?? new List<SampleClass>();
                parent.Children.AddRange(children);
            }
        );

        Assert.Single(result);
        Assert.Equal(2, result[0].Children.Count);
    }

    [Fact]
    public void ToTree_ReturnsEmptyListForEmptyInputList()
    {
        var list = new List<SampleClass>();

        List<SampleClass>? result = list.ToTree(
            (_, child) => child.ParentId == 0,
            (parent, child) => parent.Id == child.ParentId,
            (parent, children) =>
            {
                parent.Children = parent.Children ?? new List<SampleClass>();
                parent.Children.AddRange(children);
            }
        );

        Assert.Empty(result);
    }

    private class SampleClass
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = null!;
        public List<SampleClass> Children { get; set; } = new List<SampleClass>();
    }
}
