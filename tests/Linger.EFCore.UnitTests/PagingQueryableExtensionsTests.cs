namespace Linger.EFCore.Tests;

public class PagingQueryableExtensionsTests
{
    private readonly IQueryable<int> _testData;

    public PagingQueryableExtensionsTests()
    {
        _testData = Enumerable.Range(1, 10).AsQueryable();
    }

    [Theory]
    [InlineData(0, 5, new[] { 1, 2, 3, 4, 5 })]
    [InlineData(5, 3, new[] { 6, 7, 8 })]
    [InlineData(8, 5, new[] { 9, 10 })]
    public void PageBy_ShouldReturnCorrectItems(int skipCount, int maxResultCount, int[] expectedResult)
    {
        // Act
        var result = _testData.PageBy(skipCount, maxResultCount).ToArray();

        // Assert
        Assert.Equal(expectedResult.Length, result.Length);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void PageBy_WithZeroMaxResult_ShouldReturnEmptyCollection()
    {
        // Act
        var result = _testData.PageBy(0, 0).ToArray();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(true, 5, new[] { 1, 3, 5, 7, 9 })]
    [InlineData(false, 10, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
    public void WhereIf_ShouldFilterConditionally(bool condition, int expectedCount, int[] expectedResult)
    {
        // Act
        var result = _testData
            .WhereIf(condition, x => x % 2 == 1)
            .ToArray();

        // Assert
        Assert.Equal(expectedCount, result.Length);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void WhereIf_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _testData.WhereIf(true, null!));
    }

    [Fact]
    public void WhereIf_WithFalseCondition_ShouldNotModifyQuery()
    {
        // Arrange
        var originalData = _testData.ToArray();

        // Act
        var result = _testData
            .WhereIf(false, x => x > 100)
            .ToArray();

        // Assert
        Assert.Equal(originalData.Length, result.Length);
        Assert.Equal(originalData, result);
    }
}
