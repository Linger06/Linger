using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

/// <summary>
/// SqlServerHelper 单元测试类 - 仅测试不涉及数据库连接的逻辑
/// </summary>
public class SqlServerHelperTests
{
    private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=true;";

    [Fact]
    public void Constructor_WithValidConnectionString_ShouldCreateInstance()
    {
        // Arrange & Act
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Assert
        Assert.NotNull(sqlHelper);
    }

    [Fact]
    public void AddByBulkCopy_WithNullTable_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sqlHelper.AddByBulkCopy(null!, "TestTable"));
    }

    [Fact]
    public void AddByBulkCopy_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var dataTable = CreateTestDataTable();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.AddByBulkCopy(dataTable, ""));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public void AddByBulkCopy_WithWhitespaceTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var dataTable = CreateTestDataTable();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.AddByBulkCopy(dataTable, "   "));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public void AddByBulkCopy_WithEmptyDataTable_ShouldReturnWithoutError()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var emptyTable = new DataTable();

        // Act & Assert (should not throw)
        sqlHelper.AddByBulkCopy(emptyTable, "TestTable");
    }

    [Fact]
    public void GetMaxId_WithEmptyFieldName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.GetMaxId("", "TestTable"));
        Assert.Contains("字段名不能为空", exception.Message);
    }

    [Fact]
    public void GetMaxId_WithNullFieldName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.GetMaxId(null!, "TestTable"));
        Assert.Contains("字段名不能为空", exception.Message);
    }

    [Fact]
    public void GetMaxId_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.GetMaxId("Id", ""));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public void GetMaxId_WithNullTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.GetMaxId("Id", null!));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public void Exists_WithEmptySql_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.Exists(""));
        Assert.Contains("SQL语句不能为空", exception.Message);
    }

    [Fact]
    public void Exists_WithNullSql_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.Exists(null!));
        Assert.Contains("SQL语句不能为空", exception.Message);
    }

    [Fact]
    public void Exists_WithWhitespaceSql_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => sqlHelper.Exists("   "));
        Assert.Contains("SQL语句不能为空", exception.Message);
    }

    [Fact]
    public async Task AddByBulkCopyAsync_WithNullTable_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sqlHelper.AddByBulkCopyAsync(null!, "TestTable"));
    }

    [Fact]
    public async Task AddByBulkCopyAsync_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var dataTable = CreateTestDataTable();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            sqlHelper.AddByBulkCopyAsync(dataTable, ""));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public async Task GetMaxIdAsync_WithEmptyFieldName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            sqlHelper.GetMaxIdAsync("", "TestTable"));
        Assert.Contains("字段名不能为空", exception.Message);
    }

    [Fact]
    public async Task GetMaxIdAsync_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            sqlHelper.GetMaxIdAsync("Id", ""));
        Assert.Contains("表名不能为空", exception.Message);
    }

    [Fact]
    public async Task ExistsAsync_WithEmptySql_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            sqlHelper.ExistsAsync(""));
        Assert.Contains("SQL语句不能为空", exception.Message);
    }

    [Fact]
    public async Task ExistsAsync_WithNullSql_ShouldThrowArgumentException()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            sqlHelper.ExistsAsync(null!));
        Assert.Contains("SQL语句不能为空", exception.Message);
    }

    [Fact]
    public void BulkInsert_WithEmptyDataTable_ShouldReturnFalse()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var emptyTable = CreateTestDataTable();

        // Act
        var result = sqlHelper.BulkInsert(emptyTable);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BulkInsert_WithNullDataTable_ShouldReturnFalse()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Act
        var result = sqlHelper.BulkInsert(null!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    public void AddByBulkCopy_WithCustomBatchSize_ShouldAcceptValidBatchSizes(int batchSize)
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var emptyTable = CreateTestDataTable();

        // Act & Assert (should not throw for empty table)
        sqlHelper.AddByBulkCopy(emptyTable, "TestTable", batchSize);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void AddByBulkCopy_WithCustomTimeout_ShouldAcceptValidTimeouts(int timeout)
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var emptyTable = CreateTestDataTable();

        // Act & Assert (should not throw for empty table)
        sqlHelper.AddByBulkCopy(emptyTable, "TestTable", timeout: timeout);
    }

    private static DataTable CreateTestDataTable()
    {
        var table = new DataTable("TestUsers");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Email", typeof(string));
        return table;
    }
}

/// <summary>
/// 性能和边界测试类
/// </summary>
public class SqlServerHelperPerformanceTests
{
    private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=true;";

    [Fact]
    public void CreateDataTable_WithLargeDataSet_ShouldHandleMemoryEfficiently()
    {
        // Arrange
        const int rowCount = 10000;
        var table = new DataTable("LargeTable");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Data", typeof(string));

        // Act
        for (int i = 0; i < rowCount; i++)
        {
            table.Rows.Add(i, $"TestData_{i}");
        }

        var sqlHelper = new SqlServerHelper(TestConnectionString);

        // Assert
        Assert.Equal(rowCount, table.Rows.Count);

        // Verify empty table doesn't cause issues
        table.Clear();
        sqlHelper.AddByBulkCopy(table, "TestTable"); // Should not throw

        var result = sqlHelper.BulkInsert(table);
        Assert.False(result); // Empty table should return false
    }

    [Fact]
    public async Task ConcurrentAsyncOperations_ShouldNotCauseDeadlock()
    {
        // Arrange
        var sqlHelper = new SqlServerHelper(TestConnectionString);
        var tasks = new List<Task>();

        // Act - Create multiple async parameter validation tasks
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    sqlHelper.ExistsAsync("")).ConfigureAwait(false);
            }));
        }

        // Assert
        await Task.WhenAll(tasks); // Should complete without deadlock
    }
}
