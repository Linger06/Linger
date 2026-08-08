using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class SqlServerHelperTests
{
    private const string ConnectionString =
        "Server=localhost;Database=TestDb;Integrated Security=true;TrustServerCertificate=true;";

    [Fact]
    public void Constructor_WithValidConnectionString_ShouldCreateInstance()
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.NotNull(helper);
        Assert.IsAssignableFrom<IDatabase>(helper);
        Assert.IsAssignableFrom<IBulkInsert>(helper);
    }

    [Fact]
    public void BulkInsert_WithNullTable_ShouldThrowArgumentNullException()
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.Throws<ArgumentNullException>(() => helper.BulkInsert(null!, "dbo.Users"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BulkInsert_WithInvalidTableName_ShouldThrowArgumentException(string? tableName)
    {
        var helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        Assert.ThrowsAny<ArgumentException>(() => helper.BulkInsert(table, tableName!));
    }

    [Theory]
    [InlineData("dbo.")]
    [InlineData(".Users")]
    [InlineData("dbo..Users")]
    [InlineData("dbo.Users;DROP TABLE Users")]
    public void BulkInsert_WithUnsafeTableName_ShouldThrowArgumentException(string tableName)
    {
        var helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        Assert.Throws<ArgumentException>(() => helper.BulkInsert(table, tableName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BulkInsert_WithInvalidBatchSize_ShouldThrowArgumentOutOfRangeException(int batchSize)
    {
        var helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            helper.BulkInsert(table, "dbo.Users", batchSize));
    }

    [Fact]
    public void BulkInsert_WithNegativeTimeout_ShouldThrowArgumentOutOfRangeException()
    {
        var helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            helper.BulkInsert(table, "dbo.Users", timeout: -1));
    }

    [Fact]
    public void BulkInsert_WithEmptyTable_ShouldReturnZeroWithoutOpeningConnection()
    {
        IBulkInsert helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        var result = helper.BulkInsert(table, "dbo.Users", batchSize: 250, timeout: 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task BulkInsertAsync_WithEmptyTable_ShouldReturnZeroWithoutOpeningConnection()
    {
        IBulkInsert helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();

        var result = await helper.BulkInsertAsync(table, "dbo.Users", batchSize: 250, timeout: 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void BulkInsert_InsideAmbientTransaction_ShouldThrowInvalidOperationException()
    {
        using var helper = new SqlServerHelper(ConnectionString);
        var connection = new RecordingDbConnection("Transaction-Conn");
        DbTransaction transaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(connection);
        PropertyInfo? transactionProperty = typeof(BaseDatabase).GetProperty(
            "Trans",
            BindingFlags.Instance | BindingFlags.NonPublic);
        transactionProperty!.SetValue(helper, transaction);

        Assert.Throws<InvalidOperationException>(() => helper.BulkInsert(new DataTable(), "dbo.Users"));
    }

    [Fact]
    public void CreateBulkCopy_WithNamedColumns_ShouldMapColumnsByName()
    {
        var helper = new SqlServerHelper(ConnectionString);
        var table = new DataTable();
        _ = table.Columns.Add("Name", typeof(string));
        _ = table.Columns.Add("Id", typeof(int));
        MethodInfo? method = typeof(SqlServerHelper).GetMethod(
            "CreateBulkCopy",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);
        using var bulk = Assert.IsType<SqlBulkCopy>(method!.Invoke(helper, [table, "[dbo].[Users]", 1000, 100]));

        Assert.Equal(2, bulk.ColumnMappings.Count);
        Assert.Equal("Name", bulk.ColumnMappings[0].SourceColumn);
        Assert.Equal("Name", bulk.ColumnMappings[0].DestinationColumn);
        Assert.Equal("Id", bulk.ColumnMappings[1].SourceColumn);
        Assert.Equal("Id", bulk.ColumnMappings[1].DestinationColumn);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(3_000_000_000L)]
    public void GetNextId_WithOutOfRangeValue_ShouldThrowOverflowException(object maxValue)
    {
        MethodInfo? method = typeof(SqlServerHelper).GetMethod(
            "GetNextId",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        var exception = Assert.Throws<TargetInvocationException>(() => method!.Invoke(null, [maxValue]));
        Assert.IsType<OverflowException>(exception.InnerException);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData(41, 42)]
    public void GetNextId_WithValidValue_ShouldReturnNextId(object? maxValue, int expected)
    {
        MethodInfo? method = typeof(SqlServerHelper).GetMethod(
            "GetNextId",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        Assert.Equal(expected, method!.Invoke(null, [maxValue]));
    }

    [Theory]
    [InlineData(null, "Users")]
    [InlineData("", "Users")]
    [InlineData("Id", null)]
    [InlineData("Id", "")]
    [InlineData("dbo.Id", "Users")]
    [InlineData("Id", "dbo.")]
    public void GetMaxId_WithInvalidIdentifier_ShouldThrowArgumentException(string? fieldName,
        string? tableName)
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.GetMaxId(fieldName!, tableName!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TableExists_WithInvalidTableName_ShouldThrowArgumentException(string? tableName)
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.TableExists(tableName!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FindCountBySql_WithInvalidSql_ShouldThrowArgumentException(string? sql)
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.FindCountBySql(sql!));
    }

    [Fact]
    public void QueryInBatches_WithEmptyParameters_ShouldReturnEmptyDataTable()
    {
        var helper = new SqlServerHelper(ConnectionString);

        DataTable result = helper.QueryInBatches("SELECT 1 WHERE 1 IN ({0})", []);

        Assert.Empty(result.Rows);
        Assert.Empty(result.Columns);
    }

    [Fact]
    public void CommandTimeout_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        var helper = new SqlServerHelper(ConnectionString);

        Assert.Throws<ArgumentOutOfRangeException>(() => helper.CommandTimeout = -1);
    }
}
