using System.Linq.Expressions;
using Linger.Enums;
using Linger.SharedKernel;

namespace Linger.UnitTests.Helper;

public class ExpressionHelperTests2
{
    private class Student
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public ClassRoom? Room { get; set; }
    }

    private class ClassRoom
    {
        public string? RoomName { get; set; }

        public Address? RoomAddress { get; set; }
    }

    private class Address
    {
        public string? Location { get; set; }
    }

    [Fact]
    public void GetOrderBy_WithValidSortList_ReturnsOrderedQueryable()
    {
        var sortList = new List<SortInfo>
        {
            new SortInfo { Property = "Name", Direction = SortDir.Asc },
            new SortInfo { Property = "Age", Direction = SortDir.Desc }
        };

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc = ExpressionHelper.GetOrderBy<Student>(sortList);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 },
            new Student { Name = "John", Age = 20 }
        }.AsQueryable();

        Assert.NotNull(orderByFunc);

        var orderedData = orderByFunc(data).ToList();

        Assert.Equal("Jane", orderedData[0].Name);
        Assert.Equal("John", orderedData[1].Name);
        Assert.Equal(30, orderedData[1].Age);
        Assert.Equal(20, orderedData[2].Age);
    }

    [Fact]
    public void GetOrderBy_WithNullSortList_ReturnsNull()
    {
        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc = ExpressionHelper.GetOrderBy<Student>(null);
        Assert.Null(orderByFunc);
    }

    [Fact]
    public void GetOrderBy_WithEmptySortList_ReturnsNull()
    {
        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc = ExpressionHelper.GetOrderBy<Student>(new List<SortInfo>());
        Assert.Null(orderByFunc);
    }

    [Fact]
    public void BuildLambda_WithValidConditions_ReturnsCorrectExpression()
    {
        var conditions = new List<Condition>
        {
            new Condition { Field = "Name", Op = CompareOperator.Equals, Value = "John" },
            new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 20 }
        };

        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildLambda<Student>(conditions);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 },
            new Student { Name = "John", Age = 20 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Single(filteredData);
        Assert.Equal("John", filteredData[0].Name);
        Assert.Equal(30, filteredData[0].Age);
    }

    [Fact]
    public void BuildLambda_WithNullConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildLambda<Student>(null);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void BuildLambda_WithEmptyConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildLambda<Student>(new List<Condition>());
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void BuildAndAlsoLambda_WithValidConditions_ReturnsCorrectExpression()
    {
        var conditions = new List<Condition>
        {
            new Condition { Field = "Name", Op = CompareOperator.Equals, Value = "John" },
            new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 20 }
        };

        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildAndAlsoLambda<Student>(conditions);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 },
            new Student { Name = "John", Age = 20 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Single(filteredData);
        Assert.Equal("John", filteredData[0].Name);
        Assert.Equal(30, filteredData[0].Age);
    }

    [Fact]
    public void BuildAndAlsoLambda_WithNullConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildAndAlsoLambda<Student>(null);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void BuildAndAlsoLambda_WithEmptyConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildAndAlsoLambda<Student>(new List<Condition>());
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void BuildOrElseLambda_WithValidConditions_ReturnsCorrectExpression()
    {
        var conditions = new List<Condition>
        {
            new Condition { Field = "Name", Op = CompareOperator.Equals, Value = "John" },
            new Condition { Field = "Age", Op = CompareOperator.LessThan, Value = 25 }
        };

        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildOrElseLambda<Student>(conditions);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 },
            new Student { Name = "John", Age = 20 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
        Assert.Equal("John", filteredData[0].Name);
        Assert.Equal(30, filteredData[0].Age);
        Assert.Equal("John", filteredData[1].Name);
        Assert.Equal(20, filteredData[1].Age);
    }

    [Fact]
    public void BuildOrElseLambda_WithNullConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildOrElseLambda<Student>(null);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void BuildOrElseLambda_WithEmptyConditions_ReturnsTrueExpression()
    {
        Expression<Func<Student, bool>>? filterLambda = ExpressionHelper.BuildOrElseLambda<Student>(new List<Condition>());
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 }
        }.AsQueryable();

        var filteredData = data.Where(filterLambda).ToList();

        Assert.Equal(2, filteredData.Count);
    }

    [Fact]
    public void GetOrderBy_WithValidColumnsAndDirections_ReturnsOrderedQueryable()
    {
        var orderColumns = new List<string> { "Name", "Age" };
        var orderDirs = new List<string> { "asc", "desc" };

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc = ExpressionHelper.GetOrderBy<Student>(orderColumns, orderDirs);
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 },
            new Student { Name = "Jane", Age = 25 },
            new Student { Name = "John", Age = 20 }
        }.AsQueryable();

        Assert.NotNull(orderByFunc);
        var orderedData = orderByFunc(data).ToList();

        Assert.Equal("Jane", orderedData[0].Name);
        Assert.Equal("John", orderedData[1].Name);
        Assert.Equal(30, orderedData[1].Age);
        Assert.Equal(20, orderedData[2].Age);
    }

    [Fact]
    public void GetOrderBy_WithInvalidDirection_ThrowsArgumentException()
    {
        var orderColumns = new List<string> { "Name" };
        var orderDirs = new List<string> { "anything" };

        Assert.Throws<ArgumentException>(() => ExpressionHelper.GetOrderBy<Student>(orderColumns, orderDirs));
    }

    [Fact]
    public void GetOrderBy_WithNoValidColumnsAndDirections_ReturnsOrderedQueryable()
    {
        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 ,Room = new ClassRoom{ RoomName="A"}},
            new Student { Name = "Jane", Age = 25 ,Room = new ClassRoom{ RoomName="C",RoomAddress = new Address{ Location="2"}}},
            new Student { Name = "John", Age = 20 ,Room = new ClassRoom{ RoomName="B",RoomAddress = new Address{ Location="1"}}},
            new Student { Name = "John", Age = 10 ,Room = new ClassRoom{ RoomName="B",RoomAddress = new Address{ Location="2"}}},
            new Student { Name = "Jane", Age = 20 }
        }.AsQueryable();

        var orderColumns0 = new List<string> { "Parent", "Age" };
        var orderDirs0 = new List<string> { "asc", "desc" };

        Assert.Throws<InvalidOperationException>(() => ExpressionHelper.GetOrderBy<Student>(orderColumns0, orderDirs0));
    }

    [Fact]
    public void GetOrderBy_WithValidColumnsChildPropertyAndDirections_ReturnsOrderedQueryable()
    {
        var orderColumns = new List<string> { "Room.RoomName", "Age" };
        var orderDirs = new List<string> { "asc", "desc" };

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc = ExpressionHelper.GetOrderBy<Student>(orderColumns, orderDirs);

        IQueryable<Student>? data = new List<Student>
        {
            new Student { Name = "John", Age = 30 ,Room = new ClassRoom{ RoomName="A"}},
            new Student { Name = "Jane", Age = 25 ,Room = new ClassRoom{ RoomName="C"}},
            new Student { Name = "John", Age = 20 ,Room = new ClassRoom{ RoomName="B"}},
            new Student { Name = "John", Age = 10 ,Room = new ClassRoom{ RoomName="B"}}
        }.AsQueryable();

        Assert.NotNull(orderByFunc);
        var orderedData = orderByFunc(data).ToList();

        Assert.Equal("John", orderedData[0].Name);
        Assert.Equal("Jane", orderedData[3].Name);
        Assert.Equal(20, orderedData[1].Age);
        Assert.Equal(10, orderedData[2].Age);
    }

    [Fact]
    public void GetOrderBy_WithValidColumnsGrandSonPropertyAndDirections_ReturnsOrderedQueryable()
    {
        IQueryable<Student>? data = new List<Student>
        {
            //new TestClass { Name = "Jane", Age = 20 },
            //new TestClass { Name = "John", Age = 30 ,Child = new TestClass{ Name="A"}},
            new Student { Name = "Jane", Age = 25 ,Room = new ClassRoom{ RoomName="C",RoomAddress = new Address{ Location="2"}}},
            new Student { Name = "John", Age = 20 ,Room = new ClassRoom{ RoomName="B",RoomAddress = new Address{ Location="1"}}},
            new Student { Name = "John", Age = 10 ,Room = new ClassRoom{ RoomName="B",RoomAddress = new Address{ Location="2"}}}
        }.AsQueryable();

        var orderColumns2 = new List<string> { "Room.RoomAddress.Location", "Age" };
        var orderDirs2 = new List<string> { "asc", "desc" };

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderByFunc2 = ExpressionHelper.GetOrderBy<Student>(orderColumns2, orderDirs2);
        Assert.NotNull(orderByFunc2);
        var orderedData2 = orderByFunc2(data).ToList();

        Assert.Equal("John", orderedData2[0].Name);
        Assert.Equal("Jane", orderedData2[1].Name);
        Assert.Equal("John", orderedData2[2].Name);
        Assert.Equal(20, orderedData2[0].Age);
        Assert.Equal(25, orderedData2[1].Age);
        Assert.Equal(10, orderedData2[2].Age);
    }

    private class TestClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }

        public DemoClass<string>? Demo { get; set; }
    }

    private class DemoClass<T>
    {
        public T? DemoName { get; set; }
    }

    [Fact]
    public void GetOrderBy_WithValidColumnsChildPropertyWithGenericAndDirections_ReturnsOrderedQueryable()
    {
        IQueryable<TestClass>? data = new List<TestClass>
        {
            new TestClass { Name = "Jane", Age = 25 ,Demo = new DemoClass<string>{ DemoName="C"}},
            new TestClass { Name = "John", Age = 20 ,Demo = new DemoClass<string>{ DemoName="B"}},
            new TestClass { Name = "John", Age = 10 ,Demo = new DemoClass<string>{ DemoName="B"}}
        }.AsQueryable();

        var orderColumns2 = new List<string> { "Demo.DemoName", "Age" };
        var orderDirs2 = new List<string> { "asc", "desc" };

        Func<IQueryable<TestClass>, IOrderedQueryable<TestClass>>? orderByFunc2 = ExpressionHelper.GetOrderBy<TestClass>(orderColumns2, orderDirs2);
        Assert.NotNull(orderByFunc2);
        var orderedData2 = orderByFunc2(data).ToList();

        Assert.Equal("John", orderedData2[0].Name);
        Assert.Equal("John", orderedData2[1].Name);
        Assert.Equal("Jane", orderedData2[2].Name);
        Assert.Equal(20, orderedData2[0].Age);
        Assert.Equal(10, orderedData2[1].Age);
        Assert.Equal(25, orderedData2[2].Age);
    }
}

public class BaseSearchTests
{
    private sealed class Item
    {
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public int Rank { get; set; }
    }

    private sealed class ItemSearch : BaseSearch
    {
        public int Rank { get; set; }
    }

    [Theory]
    [InlineData(null, "match", true)]
    [InlineData("match", null, true)]
    [InlineData(null, null, false)]
    public void GetSearchTextExpression_WithNullMembers_MatchesOnlyNonNullText(string? name, string? alias, bool expected)
    {
        var matches = new BaseSearch { SearchText = "mat" }.GetSearchTextExpression<Item>().Compile();

        Assert.Equal(expected, matches(new Item { Name = name, Alias = alias }));
    }

    [Theory]
    [InlineData(1, "first", "match", true)]
    [InlineData(1, "match", "second", true)]
    [InlineData(1, "other", "match", false)]
    [InlineData(1, "first", "other", false)]
    [InlineData(2, "first", "match", false)]
    [InlineData(1, "blocked", "second-match", false)]
    public void GetSearchExpression_CombinesModelTextAndConditionGroups(int rank, string name, string alias, bool expected)
    {
        var search = new ItemSearch
        {
            Rank = 1,
            SearchText = "match",
            SearchParameters = [new Condition { Field = nameof(Item.Name), Op = CompareOperator.NotEquals, Value = "blocked" }],
            SearchOrParameters =
            [
                new Condition { Field = nameof(Item.Name), Op = CompareOperator.Equals, Value = "first" },
                new Condition { Field = nameof(Item.Alias), Op = CompareOperator.StartsWith, Value = "second" }
            ]
        };

        var matches = search.GetSearchExpression<Item>().Compile();

        Assert.Equal(expected, matches(new Item { Rank = rank, Name = name, Alias = alias }));
    }
}
