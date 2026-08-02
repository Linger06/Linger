using System.Data;
using System.Data.SQLite;
using Xunit;

namespace Linger.DataAccess.Sqlite.UnitTests;

public sealed class SqliteHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _databasePath;
    private readonly SqliteHelper _helper;

    public SqliteHelperTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"Linger.Sqlite.{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
        _databasePath = Path.Combine(_testDirectory, "test.db");
        _helper = SqliteHelper.CreateFileDatabase(_databasePath);
        _helper.ExecuteBySql(
            "CREATE TABLE products (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Price REAL NOT NULL)");
        _helper.ExecuteBySql("INSERT INTO products (Name, Price) VALUES (@name, @price)",
            new SQLiteParameter("@name", "Apple"), new SQLiteParameter("@price", 3.5));
        _helper.ExecuteBySql("INSERT INTO products (Name, Price) VALUES (@name, @price)",
            new SQLiteParameter("@name", "Banana"), new SQLiteParameter("@price", 2.25));
    }

    public void Dispose()
    {
        _helper.Dispose();
        SQLiteConnection.ClearAllPools();

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void CreateFileDatabase_WithValidPath_ShouldCreateInstance()
    {
        var path = Path.Combine(_testDirectory, "other.db");
        using var helper = SqliteHelper.CreateFileDatabase(path);

        Assert.NotNull(helper);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFileDatabase_WithInvalidPath_ShouldThrowArgumentException(string? path)
    {
        Assert.ThrowsAny<ArgumentException>(() => SqliteHelper.CreateFileDatabase(path!));
    }

    [Fact]
    public void Query_ShouldReturnAllResultSets()
    {
        DataSet result = _helper.Query("SELECT Id, Name FROM products ORDER BY Id; SELECT COUNT(*) AS Total FROM products;");

        Assert.Equal(2, result.Tables.Count);
        Assert.Equal(2, result.Tables[0].Rows.Count);
        Assert.Equal(2L, result.Tables[1].Rows[0]["Total"]);
    }

    [Fact]
    public void QueryTable_WithParameters_ShouldReturnMatchingRows()
    {
        DataTable result = _helper.QueryTable("SELECT Name, Price FROM products WHERE Price > @price",
            new SQLiteParameter("@price", 3));

        DataRow row = Assert.Single(result.Rows.Cast<DataRow>());
        Assert.Equal("Apple", row["Name"]);
    }

    [Fact]
    public void HasRows_WithMatchingQuery_ShouldReturnTrue()
    {
        var result = _helper.HasRows("SELECT 1 FROM products WHERE Name = @name",
            new SQLiteParameter("@name", "Apple"));

        Assert.True(result);
    }

    [Fact]
    public void FindCountBySql_WithParameters_ShouldReturnCount()
    {
        var result = _helper.FindCountBySql("SELECT COUNT(*) FROM products WHERE Price >= @price",
            new SQLiteParameter("@price", 2));

        Assert.Equal(2, result);
    }

    [Fact]
    public void FindListBySql_WithMapper_ShouldReturnValues()
    {
        List<string> result = _helper.FindListBySql("SELECT Name FROM products ORDER BY Id",
            static record => record.GetString(0));

        Assert.Equal(["Apple", "Banana"], result);
    }

    [Fact]
    public async Task FindListBySqlAsync_WithMapper_ShouldReturnValues()
    {
        List<string> result = await _helper.FindListBySqlAsync("SELECT Name FROM products ORDER BY Id",
            static record => record.GetString(0));

        Assert.Equal(["Apple", "Banana"], result);
    }

    [Fact]
    public async Task FindListBySqlAsync_WhenCanceled_ShouldThrowOperationCanceledException()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _helper.FindListBySqlAsync("SELECT Name FROM products", static record => record.GetString(0),
                cancellationToken: cancellation.Token));
    }

    [Fact]
    public void FindListBySql_WithoutMapper_ShouldMapWritableProperties()
    {
        List<ProductRecord> result = _helper.FindListBySql<ProductRecord>(
            "SELECT Id, Name, Price FROM products ORDER BY Id");

        Assert.Equal(2, result.Count);
        Assert.Equal("Apple", result[0].Name);
        Assert.Equal(2.25, result[1].Price);
    }

    [Fact]
    public async Task FindListBySqlAsync_WithoutMapper_ShouldMapWritableProperties()
    {
        List<ProductRecord> result = await _helper.FindListBySqlAsync<ProductRecord>(
            "SELECT Id, Name, Price FROM products ORDER BY Id");

        Assert.Equal(2, result.Count);
        Assert.Equal("Banana", result[1].Name);
    }

    [Fact]
    public void QueryInBatches_WithValues_ShouldMergeResults()
    {
        DataTable result = _helper.QueryInBatches(
            "SELECT Id, Name FROM products WHERE Name IN ({0}) ORDER BY Id",
            ["Apple", "Banana"], batchSize: 1);

        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void ExecuteTransaction_WithParameterizedStatements_ShouldCommitAllStatements()
    {
        var statements = new[]
        {
            new SqlStatement("INSERT INTO products (Name, Price) VALUES (@name, @price)",
                new SQLiteParameter("@name", "Cherry"), new SQLiteParameter("@price", 5)),
            new SqlStatement("UPDATE products SET Price = @price WHERE Name = @name",
                new SQLiteParameter("@price", 4), new SQLiteParameter("@name", "Apple")),
        };

        int[] affected = _helper.ExecuteTransaction(statements);

        Assert.Equal([1, 1], affected);
        Assert.Equal(3, _helper.FindCountBySql("SELECT COUNT(*) FROM products"));
    }

    [Fact]
    public void ExecuteTransaction_WhenStatementFails_ShouldRollbackAllStatements()
    {
        var statements = new[]
        {
            new SqlStatement("INSERT INTO products (Name, Price) VALUES (@name, @price)",
                new SQLiteParameter("@name", "Cherry"), new SQLiteParameter("@price", 5)),
            new SqlStatement("INSERT INTO missing_table (Name) VALUES (@name)",
                new SQLiteParameter("@name", "Failure")),
        };

        Assert.Throws<SQLiteException>(() => _helper.ExecuteTransaction(statements));
        Assert.Equal(0, _helper.FindCountBySql("SELECT COUNT(*) FROM products WHERE Name = 'Cherry'"));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithParameterizedStatements_ShouldCommitAllStatements()
    {
        var statements = new[]
        {
            new SqlStatement("INSERT INTO products (Name, Price) VALUES (@name, @price)",
                new SQLiteParameter("@name", "Cherry"), new SQLiteParameter("@price", 5)),
        };

        int[] affected = await _helper.ExecuteTransactionAsync(statements);

        Assert.Equal([1], affected);
    }

    [Fact]
    public void ExecuteTransaction_WithNullStatement_ShouldThrowArgumentNullException()
    {
        var statements = new SqlStatement[]
        {
            null!,
        };

        Assert.Throws<ArgumentNullException>(() => _helper.ExecuteTransaction(statements));
    }

    [Fact]
    public void SqlStatement_WithNullParameterArray_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlStatement("SELECT 1", null!));
    }

    [Fact]
    public void SqlStatement_WithNoParameters_ShouldUseEmptyParameterArray()
    {
        var statement = new SqlStatement("SELECT 1");

        Assert.Empty(statement.Parameters);
    }

    [Fact]
    public void ExecuteTransaction_WithParameterlessStatement_ShouldExecute()
    {
        var statement = new SqlStatement("INSERT INTO products (Name, Price) VALUES ('Cherry', 5)");

        int[] affected = _helper.ExecuteTransaction([statement]);

        Assert.Equal([1], affected);
    }

    [Fact]
    public void CommandTimeout_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _helper.CommandTimeout = -1);
    }

    private sealed class ProductRecord
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Price { get; set; }
    }
}
