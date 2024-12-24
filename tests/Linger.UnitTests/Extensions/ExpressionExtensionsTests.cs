namespace Linger.UnitTests.Extensions;

using System.Linq.Expressions;
using Linger.Extensions;
using Xunit;

public class ExpressionExtensionsTests
{
    [Fact]
    public void AndIf_ReturnsLeftExpression_WhenNeedIsFalse()
    {
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>>? result = left.AndIf(false, right);
        Assert.Equal(left, result);
    }

    [Fact]
    public void AndIf_ReturnsCombinedExpression_WhenNeedIsTrue()
    {
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>>? result = left.AndIf(true, right);
        Expression<Func<int, bool>>? expected = left.And(right);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void OrIf_ReturnsLeftExpression_WhenNeedIsFalse()
    {
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>>? result = left.OrIf(false, right);
        Assert.Equal(left, result);
    }

    [Fact]
    public void OrIf_ReturnsCombinedExpression_WhenNeedIsTrue()
    {
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>>? result = left.OrIf(true, right);
        Expression<Func<int, bool>>? expected = left.Or(right);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void HasOrderBy_ReturnsTrue_WhenQueryHasOrderBy()
    {
        IOrderedQueryable<int>? query = new List<int> { 1, 2, 3 }.AsQueryable().OrderBy(x => x);
        var result = query.HasOrderBy();
        Assert.True(result);
    }

    [Fact]
    public void HasOrderBy_ReturnsFalse_WhenQueryHasNoOrderBy()
    {
        IQueryable<int>? query = new List<int> { 1, 2, 3 }.AsQueryable();
        var result = query.HasOrderBy();
        Assert.False(result);
    }

    [Fact]
    public void Compose_CombinesExpressionsWithAnd()
    {
        Expression<Func<int, bool>> first = x => x > 5;
        Expression<Func<int, bool>> second = x => x < 10;
        Expression<Func<int, bool>>? result = first.Compose(second, Expression.AndAlso);
        Expression<Func<int, bool>>? expected = first.And(second);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void Compose_CombinesExpressionsWithOr()
    {
        Expression<Func<int, bool>> first = x => x > 5;
        Expression<Func<int, bool>> second = x => x < 10;
        Expression<Func<int, bool>>? result = first.Compose(second, Expression.OrElse);
        Expression<Func<int, bool>>? expected = first.Or(second);
        Assert.Equal(expected.ToString(), result.ToString());
    }
}