using System.Linq.Expressions;
using System.Text.Json;

using Linger.Enums;

namespace Linger.UnitTests.Helper;

public class ExpressionHelperTests
{
    private class TestClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public int? Score { get; set; }
        public Guid Id { get; set; }
        public Status Status { get; set; }
        public TimeSpan Duration { get; set; }
    }

    private enum Status
    {
        Active,
        Disabled
    }

    [Fact]
    public void True_ReturnsTrueExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.True<TestClass>();
        Func<TestClass, bool>? compiled = expression.Compile();
        Assert.True(compiled(new TestClass()));
    }

    [Fact]
    public void False_ReturnsFalseExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.False<TestClass>();
        Func<TestClass, bool>? compiled = expression.Compile();
        Assert.False(compiled(new TestClass()));
    }

    [Fact]
    public void GetOrderExpression_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, string>>? expression = ExpressionHelper.GetOrderExpression<TestClass, string>("Name");
        Func<TestClass, string>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "John" };
        Assert.Equal("John", compiled(testObj));
    }

    [Fact]
    public void OrderBy_WithPropertyNameAndDirection_ReturnsOrderedEnumerable()
    {
        var data = new List<TestClass>
        {
            new TestClass { Name = "John", Age = 30 },
            new TestClass { Name = "Jane", Age = 25 },
            new TestClass { Name = "John", Age = 20 }
        };

        var orderedData = data.OrderBy("Name Asc").ToList();
        Assert.Equal("Jane", orderedData[0].Name);
        Assert.Equal("John", orderedData[1].Name);
        Assert.Equal("John", orderedData[2].Name);

        orderedData = data.OrderBy("Age Desc").ToList();
        Assert.Equal(30, orderedData[0].Age);
        Assert.Equal(25, orderedData[1].Age);
        Assert.Equal(20, orderedData[2].Age);

        orderedData = data.OrderBy("Age").ToList();
        Assert.Equal(20, orderedData[0].Age);
        Assert.Equal(25, orderedData[1].Age);
        Assert.Equal(30, orderedData[2].Age);
    }

    [Fact]
    public void OrderBy_WithInvalidDirection_ThrowsArgumentException()
    {
        var data = new List<TestClass> { new() { Name = "John" } };

        Assert.Throws<ArgumentException>(() => data.OrderBy("Name anything").ToList());
    }

    [Fact]
    public void CreateEqual_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateEqual<TestClass>("Name", "John");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "John" };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateEqual_ConvertsStringToGuid()
    {
        var id = Guid.NewGuid();

        var predicate = ExpressionHelper.CreateEqual<TestClass>(nameof(TestClass.Id), id.ToString()).Compile();

        Assert.True(predicate(new TestClass { Id = id }));
    }

    [Fact]
    public void CreateEqual_ConvertsStringToEnum()
    {
        var predicate = ExpressionHelper.CreateEqual<TestClass>(nameof(TestClass.Status), nameof(Status.Active)).Compile();

        Assert.True(predicate(new TestClass { Status = Status.Active }));
    }

    [Fact]
    public void CreateEqual_ConvertsStringToTimeSpan()
    {
        var predicate = ExpressionHelper.CreateEqual<TestClass>(nameof(TestClass.Duration), "01:30:00").Compile();

        Assert.True(predicate(new TestClass { Duration = TimeSpan.FromMinutes(90) }));
    }

    [Fact]
    public void CreateEqualProperty_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateEqualProperty<TestClass>("Name", "Name");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "John" };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateNotEqualProperty_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateNotEqualProperty<TestClass>("Name", "Name");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "John" };
        Assert.False(compiled(testObj));
    }

    [Fact]
    public void CreateNotEqual_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateNotEqual<TestClass>("Name", "John");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "Jane" };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateGreaterThan_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateGreaterThan<TestClass>("Age", "25");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 30 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateLessThan_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateLessThan<TestClass>("Age", "25");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 20 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateGreaterThanOrEqual_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateGreaterThanOrEqual<TestClass>("Age", "25");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 25 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void CreateLessThanOrEqual_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.CreateLessThanOrEqual<TestClass>("Age", "25");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 25 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void GetContains_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.GetContains<TestClass>("Name", "John");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "John" };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void GetNotContains_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.GetNotContains<TestClass>("Name", "John");
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Name = "Jane" };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void GetInArray_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.GetIn<TestClass, int>("Age", new[] { 20, 30 });
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 30 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void GetNotInArray_ReturnsCorrectExpression()
    {
        Expression<Func<TestClass, bool>>? expression = ExpressionHelper.GetNotIn<TestClass, int>("Age", new[] { 20, 30 });
        Func<TestClass, bool>? compiled = expression.Compile();
        var testObj = new TestClass { Age = 40 };
        Assert.True(compiled(testObj));
    }

    [Fact]
    public void BuildLambda_WithNullNullableValue_DoesNotThrow()
    {
        var condition = new Condition
        {
            Field = nameof(TestClass.Score),
            Op = CompareOperator.GreaterThan,
            Value = 10
        };
        var predicate = ExpressionHelper.BuildLambda<TestClass>([condition]).Compile();

        Assert.False(predicate(new TestClass { Score = null }));
        Assert.True(predicate(new TestClass { Score = 11 }));
    }

    [Fact]
    public void GetContains_WithNullProperty_ReturnsFalse()
    {
        var predicate = ExpressionHelper.GetContains<TestClass>(nameof(TestClass.Name), "value").Compile();

        Assert.False(predicate(new TestClass { Name = null }));
    }

    [Fact]
    public void GetNotContains_WithNullProperty_ReturnsTrue()
    {
        var predicate = ExpressionHelper.GetNotContains<TestClass>(nameof(TestClass.Name), "value").Compile();

        Assert.True(predicate(new TestClass { Name = null }));
    }

    [Fact]
    public void BuildLambda_WithEmptyField_ThrowsArgumentException()
    {
        var condition = new Condition { Field = string.Empty, Op = CompareOperator.Equals, Value = 1 };

        Assert.Throws<ArgumentException>(() => ExpressionHelper.BuildLambda<TestClass>([condition]));
    }

    [Fact]
    public void Condition_DeserializeWithoutField_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Condition>("{}"));
    }
}
