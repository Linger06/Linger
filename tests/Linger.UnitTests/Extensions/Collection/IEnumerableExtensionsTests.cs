﻿using System.Reflection;

namespace Linger.UnitTests.Extensions.Collection;

public class IEnumerableExtensionsTests
{
    [Fact]
    public void ForEach_Action_ShouldApplyActionToEachElement()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        var result = new List<int>();

        list.ForEach(x => result.Add(x * 2));

        Assert.Equal(new List<int> { 2, 4, 6 }, result);
    }

    [Fact]
    public void ForEach_ActionWithIndex_ShouldApplyActionToEachElementWithIndex()
    {
        IEnumerable<int> list = new List<int> { 1, 2, 3 };
        var result = new List<int>();

        list.ForEach((x, index) => result.Add(x * index));

        Assert.Equal(new List<int> { 0, 2, 6 }, result);
    }

#if !NETFRAMEWORK || NET462_OR_GREATER

    [Fact]
    public void HasAttribute_ShouldReturnTrueIfAttributeExists()
    {
        Type? type = typeof(SampleClass);
        IList<CustomAttributeData>? attributes = type.GetCustomAttributesData();

        Assert.True(attributes.HasAttribute(typeof(SampleAttribute)));
    }

#endif

#if !NET6_0_OR_GREATER
    [Fact]
    public void DistinctBy_ShouldReturnDistinctElementsByKey()
    {
        var list = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 1, Name = "B" },
            new SampleClass { Id = 2, Name = "C" }
        };

        var distinctList = list.DistinctBy(x => x.Id).ToList();

        Assert.Equal(2, distinctList.Count);
        Assert.Contains(distinctList, x => x.Name == "A");
        Assert.Contains(distinctList, x => x.Name == "C");
    }
#endif

    [Fact]
    public void LeftOuterJoin_ShouldReturnLeftOuterJoin()
    {
        var left = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" }
        };
        var right = new List<SampleClass>
        {
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" },
            new SampleClass { Id = 4, Name = "D" }
        };

        var result = left.LeftOuterJoin(right, l => l.Id, r => r.Id, (l, r) => new { l, r }).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.l.Id == 1 && x.r == null);
        Assert.Contains(result, x => x.l.Id == 2 && x.r!.Id == 2);
        Assert.Contains(result, x => x.l.Id == 3 && x.r!.Id == 3);
    }

    [Fact]
    public void LeftOuterJoin_ShouldReturnLeftOuterJoinWihtIntType()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.LeftOuterJoin(list2, x => x, y => y, (x, y) => new { x, y });

        jointList.Should().HaveCount(3);

        jointList.ElementAt(0).x.Should().Be(1);
        jointList.ElementAt(0).y.Should().BeNull();

        jointList.ElementAt(1).x.Should().Be(2);
        jointList.ElementAt(1).y.Should().BeNull();

        jointList.ElementAt(2).x.Should().Be(3);
        jointList.ElementAt(2).y.Should().Be(3);
    }

    [Fact]
    public void RightOuterJoin_ShouldReturnRightOuterJoin()
    {
        var left = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" }
        };
        var right = new List<SampleClass>
        {
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" },
            new SampleClass { Id = 4, Name = "D" }
        };

        var result = left.RightOuterJoin(right, l => l.Id, r => r.Id, (l, r) => new { l, r }).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.l == null && x.r.Id == 4);
        Assert.Contains(result, x => x.l!.Id == 2 && x.r.Id == 2);
        Assert.Contains(result, x => x.l!.Id == 3 && x.r.Id == 3);
    }

    [Fact]
    public void RightOuterJoin_ShouldReturnLeftOuterJoinWihtIntType()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.RightOuterJoin(list2, x => x, y => y, (x, y) => new { x, y });

        jointList.Should().HaveCount(3);

        jointList.ElementAt(0).x.Should().Be(3);
        jointList.ElementAt(0).y.Should().Be(3);

        jointList.ElementAt(1).x.Should().BeNull();
        jointList.ElementAt(1).y.Should().Be(4);

        jointList.ElementAt(2).x.Should().BeNull();
        jointList.ElementAt(2).y.Should().Be(5);
    }

    [Fact]
    public void InnerJoin_ShouldReturnInnerJoin()
    {
        var left = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" }
        };
        var right = new List<SampleClass>
        {
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" },
            new SampleClass { Id = 4, Name = "D" }
        };

        var result = left.InnerJoin(right, l => l.Id, r => r.Id, (l, r) => new { l, r }).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.l.Id == 2 && x.r.Id == 2);
        Assert.Contains(result, x => x.l.Id == 3 && x.r.Id == 3);
    }

    [Fact]
    public void InnerJoin_ShouldReturnLeftOuterJoinWihtIntType()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.InnerJoin(list2, x => x, y => y, (x, y) => new { x, y });

        jointList.Should().HaveCount(1);

        jointList.ElementAt(0).x.Should().Be(3);
        jointList.ElementAt(0).y.Should().Be(3);
    }

    [Fact]
    public void FullOuterJoin_ShouldReturnFullOuterJoin()
    {
        var left = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" }
        };
        var right = new List<SampleClass>
        {
            new SampleClass { Id = 2, Name = "B" },
            new SampleClass { Id = 3, Name = "C" },
            new SampleClass { Id = 4, Name = "D" }
        };

        var result = left.FullOuterJoin(right, l => l.Id, r => r.Id, (l, r) => new { l, r }).ToList();

        Assert.Equal(4, result.Count);
        Assert.Contains(result, x => x.l!.Id == 1 && x.r == null);
        Assert.Contains(result, x => x.l!.Id == 2 && x.r!.Id == 2);
        Assert.Contains(result, x => x.l!.Id == 3 && x.r!.Id == 3);
        Assert.Contains(result, x => x.l == null && x.r!.Id == 4);
    }

    [Fact]
    public void FullOuterJoin_ShouldReturnLeftOuterJoinWihtIntType()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.FullOuterJoin(list2, x => x, y => y, (x, y) => new { x, y });

        jointList.Should().HaveCount(5);

        jointList.ElementAt(0).x.Should().Be(1);
        jointList.ElementAt(0).y.Should().BeNull();

        jointList.ElementAt(1).x.Should().Be(2);
        jointList.ElementAt(1).y.Should().BeNull();

        jointList.ElementAt(2).x.Should().Be(3);
        jointList.ElementAt(2).y.Should().Be(3);

        jointList.ElementAt(3).x.Should().BeNull();
        jointList.ElementAt(3).y.Should().Be(4);

        jointList.ElementAt(4).x.Should().BeNull();
        jointList.ElementAt(4).y.Should().Be(5);
    }

    [Fact]
    public void Add_ShouldAddElementToEnd()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Add(4);
        var result = list.ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal(4, result.Last());
    }

    [Fact]
    public void ToDataTable_ShouldConvertListToDataTable()
    {
        var list = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" }
        };

        var dataTable = list.ToDataTable();

        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(1, dataTable.Rows[0]["Id"]);
        Assert.Equal("A", dataTable.Rows[0]["Name"]);
    }

    [Fact]
    public void ToDataTable_WithActions_ShouldConvertListToDataTable()
    {
        var list = new List<SampleClass>
        {
            new SampleClass { Id = 1, Name = "A" },
            new SampleClass { Id = 2, Name = "B" }
        };

        var dataTable = list.ToDataTable(
            (column, columnInfo) => column.ColumnName = columnInfo.PropertyName.ToUpper(),
            (row, columnInfo, item) => row[columnInfo.PropertyName.ToUpper()] = columnInfo.Property.GetValue(item)
        );

        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(1, dataTable.Rows[0]["ID"]);
        Assert.Equal("A", dataTable.Rows[0]["NAME"]);
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueIfNullOrEmpty()
    {
        List<int>? list = null;

        Assert.True(list.IsNullOrEmpty());

        list = new List<int>();

        Assert.True(list.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseIfNotEmpty()
    {
        var list = new List<int> { 1 };

        Assert.False(list.IsNullOrEmpty());
    }

    [Fact]
    public void ToSeparatedString_ShouldReturnSeparatedString()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = list.ToSeparatedString(", ", i => i.ToString());
        Assert.Equal("1, 2, 3", result);
    }

    [Fact]
    public void ToSeparatedString_ShouldReturnEmptyStringForNullEnumerable()
    {
        List<int>? list = null;
        var result = list.ToSeparatedString(", ", i => i.ToString());
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSeparatedString_ShouldReturnEmptyStringForEmptyEnumerable()
    {
        var list = new List<int>();
        var result = list.ToSeparatedString(", ", i => i.ToString());
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSeparatedString_ShouldUseDefaultSeparator()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = list.ToSeparatedString(format: i => i.ToString());
        Assert.Equal("1,2,3", result);
    }

    [Fact]
    public void ToSeparatedString_ShouldHandleNullElements()
    {
        var list = new List<string?> { "a", null, "b" };
        var result = list.ToSeparatedString(",", i => i ?? string.Empty);
        Assert.Equal("a,,b", result);
    }

    [Fact]
    public void ToSeparatedString_ShouldHandleNullFormat()
    {
        var list = new List<string?> { "a", null, "b" };
        var result = list.ToSeparatedString(format: null);
        Assert.Equal("a,,b", result);
    }

    [Sample]
    private class SampleClass
    {
        [Sample]
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    private class SampleAttribute : System.Attribute
    { }
}