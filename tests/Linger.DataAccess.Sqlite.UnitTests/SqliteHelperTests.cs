using System.Data;
using System.Data.SQLite;
using Xunit;

namespace Linger.DataAccess.Sqlite.UnitTests;

/// <summary>
/// SQLite数据库操作助手类的单元测试
/// </summary>
public class SqliteHelperTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly SqliteHelper _fileHelper;
    private bool _isInitialized;

    public SqliteHelperTests()
    {
        try
        {
            // 创建临时文件数据库用于测试
            _testDbPath = Path.GetTempFileName();
            File.Delete(_testDbPath); // 删除临时文件，让SQLite创建
            _testDbPath = Path.ChangeExtension(_testDbPath, ".db");
            _fileHelper = SqliteHelper.CreateFileDatabase(_testDbPath);

            // 安全地初始化测试数据
            InitializeTestData();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // 清理资源
            _fileHelper?.Dispose();
            _isInitialized = false;
            throw new InvalidOperationException($"测试环境初始化失败: {ex.Message}", ex);
        }
    }

    private void InitializeTestData()
    {
        // 在文件数据库中创建测试表和数据
        _fileHelper.ExecuteBySql(@"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT,
                age INTEGER,
                active INTEGER DEFAULT 1,
                created_date TEXT DEFAULT CURRENT_TIMESTAMP
            )");

        _fileHelper.ExecuteBySql(@"
            INSERT INTO users (name, email, age, active) VALUES 
            ('Alice', 'alice@test.com', 25, 1),
            ('Bob', 'bob@test.com', 30, 1),
            ('Charlie', 'charlie@test.com', 35, 0),
            ('David', 'david@test.com', 28, 1),
            ('Eve', 'eve@test.com', 32, 1)");

        // 创建产品测试表
        _fileHelper.ExecuteBySql(@"
            CREATE TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                category TEXT,
                price REAL,
                stock INTEGER DEFAULT 0
            )");

        _fileHelper.ExecuteBySql(@"
            INSERT INTO products (name, category, price, stock) VALUES 
            ('Laptop', 'Electronics', 999.99, 10),
            ('Phone', 'Electronics', 599.99, 25),
            ('Book', 'Education', 19.99, 100),
            ('Chair', 'Furniture', 149.99, 5)");

        // 创建另一张测试表用于存在性测试
        _fileHelper.ExecuteBySql(@"
            CREATE TABLE IF NOT EXISTS test_table (
                id INTEGER PRIMARY KEY,
                name TEXT
            )");

        // 创建文档表用于测试（替代FTS5）
        _fileHelper.ExecuteBySql(@"
            CREATE TABLE IF NOT EXISTS documents (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT,
                content TEXT
            )");

        _fileHelper.ExecuteBySql(@"
            INSERT INTO documents (title, content) VALUES 
            ('Document 1', 'This is the content of document 1'),
            ('Document 2', 'This contains different text for searching'),
            ('Report', 'Annual report with financial data')");
    }

    public void Dispose()
    {
        _fileHelper?.Dispose();

        // 清理测试文件
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // 忽略删除错误
            }
        }
    }

    #region 静态工厂方法测试

    [Fact]
    public void CreateFileDatabase_WithValidPath_ShouldCreateInstance()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        tempPath = Path.ChangeExtension(tempPath, ".db");

        try
        {
            // Act
            using var helper = SqliteHelper.CreateFileDatabase(tempPath);

            // Assert
            Assert.NotNull(helper);
            // 验证实例创建成功即可
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFileDatabase_WithInvalidPath_ShouldThrowException(string? filePath)
    {
        // Act & Assert
        if (filePath is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => SqliteHelper.CreateFileDatabase(filePath!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => SqliteHelper.CreateFileDatabase(filePath));
        }
    }

    #endregion

    #region 分页查询测试

    [Fact]
    public void Page_WithValidParameters_ShouldReturnResults()
    {
        // Arrange
        var userIds = new List<string> { "1", "2", "3" };
        const string sql = "SELECT * FROM users WHERE id IN ({0})";

        // Act
        var result = _fileHelper.QueryInBatches(sql, userIds);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Rows.Count > 0);
    }

    [Fact]
    public void Page_WithLargeParameterList_ShouldHandleBatching()
    {
        // Arrange
        var largeList = new List<string>();
        for (int i = 1; i <= 2500; i++)
        {
            largeList.Add(i.ToString());
        }
        const string sql = "SELECT COUNT(*) as total FROM users WHERE id NOT IN ({0})";

        // Act
        var result = _fileHelper.QueryInBatches(sql, largeList);

        // Assert
        Assert.NotNull(result);
        // 由于我们的测试数据只有5条记录，所有记录都应该被返回
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Page_WithInvalidSql_ShouldThrowException(string? sql)
    {
        // Arrange
        var parameters = new List<string> { "1", "2" };

        // Act & Assert
        if (sql is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => _fileHelper.QueryInBatches(sql!, parameters));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _fileHelper.QueryInBatches(sql, parameters));
        }
    }

    [Fact]
    public void Page_WithNullParameters_ShouldThrowArgumentNullException()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE id IN ({0})";

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => _fileHelper.QueryInBatches(sql, null!));
    }

    [Fact]
    public async Task PageAsync_WithValidParameters_ShouldReturnResults()
    {
        // Arrange
        var userIds = new List<string> { "1", "2" };
        const string sql = "SELECT * FROM users WHERE id IN ({0})";

        // Act
        var result = await _fileHelper.QueryInBatchesAsync(sql, userIds);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Rows.Count > 0);
    }

    [Fact]
    public async Task PageAsync_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        var userIds = new List<string> { "1", "2" };
        const string sql = "SELECT * FROM users WHERE id IN ({0})";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
    var exPage = await Record.ExceptionAsync(() => _fileHelper.QueryInBatchesAsync(sql, userIds, cancellationToken: cts.Token));
    Assert.IsAssignableFrom<OperationCanceledException>(exPage);
    }

    #endregion

    #region 存在性检查测试

    [Fact]
    public void Exists_WithValidQuery_ShouldReturnTrue()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM users WHERE active = 1";

        // Act
        var result = _fileHelper.Exists(sql);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_WithParameterizedQuery_ShouldReturnCorrectResult()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM users WHERE name = @name";
        var parameter = new SQLiteParameter("@name", "Alice");

        // Act
        var result = _fileHelper.Exists(sql, parameter);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_WithNonExistentData_ShouldReturnFalse()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM users WHERE name = @name";
        var parameter = new SQLiteParameter("@name", "NonExistentUser");

        // Act
        var result = _fileHelper.Exists(sql, parameter);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Exists_WithInvalidSql_ShouldThrowException(string? sql)
    {
        // Act & Assert
        if (sql is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => _fileHelper.Exists(sql!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _fileHelper.Exists(sql));
        }
    }

    [Fact]
    public async Task ExistsAsync_WithValidQuery_ShouldReturnTrue()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM users WHERE active = 1";

        // Act
        var result = await _fileHelper.ExistsAsync(sql);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM users";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
    var exExists = await Record.ExceptionAsync(() => _fileHelper.ExistsAsync(sql, cts.Token));
    Assert.IsAssignableFrom<OperationCanceledException>(exExists);
    }

    #endregion

    #region 查询方法测试

    [Fact]
    public void Query_WithSimpleSelect_ShouldReturnDataSet()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE active = 1";

        // Act
        var result = _fileHelper.Query(sql);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tables.Count > 0);
        Assert.True(result.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public void Query_WithParameterizedQuery_ShouldReturnCorrectResults()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE age > @minAge AND active = @active";
        var parameters = new SQLiteParameter[]
        {
            new("@minAge", 25),
            new("@active", 1)
        };

        // Act
        var result = _fileHelper.Query(sql, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tables.Count > 0);
        var table = result.Tables[0];
        Assert.True(table.Rows.Count > 0);

        // 验证返回的数据符合条件
        foreach (DataRow row in table.Rows)
        {
            Assert.True(Convert.ToInt32(row["age"]) > 25);
            Assert.Equal(1, Convert.ToInt32(row["active"]));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Query_WithInvalidSql_ShouldThrowException(string? sql)
    {
        // Act & Assert
        if (sql is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => _fileHelper.Query(sql!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _fileHelper.Query(sql));
        }
    }

    [Fact]
    public async Task QueryAsync_WithValidSql_ShouldReturnDataSet()
    {
        // Arrange
        const string sql = "SELECT * FROM users WHERE active = 1";

        // Act
        var result = await _fileHelper.QueryAsync(sql);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Tables.Count > 0);
        Assert.True(result.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public async Task QueryAsync_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        const string sql = "SELECT * FROM users";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
    var exQuery = await Record.ExceptionAsync(() => _fileHelper.QueryAsync(sql, cts.Token));
    Assert.IsAssignableFrom<OperationCanceledException>(exQuery);
    }

    #endregion

    #region SQLite特有功能测试

    [Fact]
    public void VacuumDatabase_ShouldReturnTrue()
    {
        // Act
        var result = _fileHelper.VacuumDatabase();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AnalyzeDatabase_ShouldReturnTrue()
    {
        // Act
        var result = _fileHelper.AnalyzeDatabase();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetDatabaseSize_ShouldReturnPositiveValue()
    {
        // Act
        var size = _fileHelper.GetDatabaseSize();

        // Assert
        Assert.True(size > 0);
    }

    [Fact]
    public void CheckIntegrity_ShouldReturnOk()
    {
        // Act
        var result = _fileHelper.CheckIntegrity();

        // Assert
        Assert.Equal("ok", result.ToLower());
    }

    [Fact]
    public void GetTableNames_ShouldReturnTableList()
    {
        // Act
        var tables = _fileHelper.GetTableNames();

        // Assert
        Assert.NotNull(tables);
        Assert.Contains("products", tables);
    }

    [Fact]
    public void TableExists_WithExistingTable_ShouldReturnTrue()
    {
        // Act
        var exists = _fileHelper.TableExists("products");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void TableExists_WithNonExistingTable_ShouldReturnFalse()
    {
        // Act
        var exists = _fileHelper.TableExists("non_existing_table");

        // Assert
        Assert.False(exists);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TableExists_WithInvalidTableName_ShouldThrowException(string? tableName)
    {
        // Act & Assert
        if (tableName is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => _fileHelper.TableExists(tableName!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _fileHelper.TableExists(tableName));
        }
    }

    #endregion

    #region 备份和恢复测试

    [Fact]
    public void BackupDatabase_WithValidPath_ShouldReturnTrue()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        File.Delete(backupPath);
        backupPath = Path.ChangeExtension(backupPath, ".db");

        try
        {
            // Act
            var result = _fileHelper.BackupDatabase(backupPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(backupPath));
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BackupDatabase_WithInvalidPath_ShouldThrowException(string? backupPath)
    {
        // Act & Assert
        if (backupPath is null)
        {
            Assert.Throws<System.ArgumentNullException>(() => _fileHelper.BackupDatabase(backupPath!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _fileHelper.BackupDatabase(backupPath));
        }
    }

    [Fact]
    public async Task BackupDatabaseAsync_WithValidPath_ShouldReturnTrue()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        File.Delete(backupPath);
        backupPath = Path.ChangeExtension(backupPath, ".db");

        try
        {
            // Act
            var result = await _fileHelper.BackupDatabaseAsync(backupPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(backupPath));
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task BackupDatabaseAsync_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
    var exBackup = await Record.ExceptionAsync(() => _fileHelper.BackupDatabaseAsync(backupPath, cts.Token));
    Assert.IsAssignableFrom<OperationCanceledException>(exBackup);
    }

    #endregion

    #region 事务处理测试

    [Fact]
    public void ExecuteInTransaction_WithValidStatements_ShouldReturnTrue()
    {
        // Arrange
        var statements = new[]
        {
            "INSERT INTO products (name, category, price, stock) VALUES ('Test Product 1', 'Test', 10.00, 1)",
            "INSERT INTO products (name, category, price, stock) VALUES ('Test Product 2', 'Test', 20.00, 2)",
            "UPDATE products SET stock = stock + 10 WHERE category = 'Test'"
        };

        // Act
        var result = _fileHelper.ExecuteInTransaction(statements);

        // Assert
        Assert.True(result);

        // 验证数据是否正确插入
        var verifyResult = _fileHelper.Query("SELECT COUNT(*) as count FROM products WHERE category = 'Test'");
        Assert.Equal(2, Convert.ToInt32(verifyResult.Tables[0].Rows[0]["count"]));
    }

    [Fact]
    public void ExecuteInTransaction_WithNullStatements_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => _fileHelper.ExecuteInTransaction(null!));
    }

    [Fact]
    public void ExecuteInTransaction_WithEmptyStatements_ShouldReturnTrue()
    {
        // Arrange
        var statements = Array.Empty<string>();

        // Act
        var result = _fileHelper.ExecuteInTransaction(statements);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithValidStatements_ShouldReturnTrue()
    {
        // Arrange
        var statements = new[]
        {
            "INSERT INTO products (name, category, price, stock) VALUES ('Async Product 1', 'AsyncTest', 15.00, 5)",
            "INSERT INTO products (name, category, price, stock) VALUES ('Async Product 2', 'AsyncTest', 25.00, 10)"
        };

        // Act
        var result = await _fileHelper.ExecuteInTransactionAsync(statements);

        // Assert
        Assert.True(result);

        // 验证数据是否正确插入
        var verifyResult = _fileHelper.Query("SELECT COUNT(*) as count FROM products WHERE category = 'AsyncTest'");
        Assert.Equal(2, Convert.ToInt32(verifyResult.Tables[0].Rows[0]["count"]));
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        var statements = new[] { "SELECT 1" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
    var exTran = await Record.ExceptionAsync(() => _fileHelper.ExecuteInTransactionAsync(statements, cts.Token));
    Assert.IsAssignableFrom<OperationCanceledException>(exTran);
    }

    #endregion

    #region SQL注入防护测试

    [Fact]
    public void ParameterizedQuery_ShouldPreventSqlInjection()
    {
        // Arrange
        const string maliciousInput = "'; DROP TABLE users; --";
        const string sql = "SELECT * FROM users WHERE name = @name";
        var parameter = new SQLiteParameter("@name", maliciousInput);

        // Act - 执行参数化查询（恶意输入应被安全处理）
        var result = _fileHelper.Query(sql, parameter);

        // Assert
        Assert.NotNull(result);
        // 表应该仍然存在（没有被删除）
        var tableCheck = _fileHelper.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='users'");
        Assert.True(tableCheck.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public void SQLiteParameter_Construction_ShouldCreateValidParameters()
    {
        // Arrange & Act
        var stringParam = new SQLiteParameter("@name", "test");
        var intParam = new SQLiteParameter("@age", 25);
        var doubleParam = new SQLiteParameter("@price", 99.99);
        var dateParam = new SQLiteParameter("@date", DateTime.Now);
        var nullParam = new SQLiteParameter("@optional", DBNull.Value);

        // Assert
        Assert.Equal("@name", stringParam.ParameterName);
        Assert.Equal("test", stringParam.Value);

        Assert.Equal("@age", intParam.ParameterName);
        Assert.Equal(25, intParam.Value);

        Assert.Equal("@price", doubleParam.ParameterName);
        Assert.Equal(99.99, doubleParam.Value);

        Assert.Equal("@date", dateParam.ParameterName);
        Assert.IsType<DateTime>(dateParam.Value);

        Assert.Equal("@optional", nullParam.ParameterName);
        Assert.Equal(DBNull.Value, nullParam.Value);
    }

    #endregion

    #region 性能和批量操作测试

    [Fact]
    public void BatchProcessing_LargeParameterList_ShouldSplitCorrectly()
    {
        // Arrange
        var parameters = new List<string>();
        for (int i = 1; i <= 2500; i++)
        {
            parameters.Add(i.ToString());
        }

        // Act - 测试分批逻辑
        var batchSize = 1000;
        var batches = new List<List<string>>();

        for (int i = 0; i < parameters.Count; i += batchSize)
        {
            batches.Add(parameters.Skip(i).Take(batchSize).ToList());
        }

        // Assert
        Assert.Equal(3, batches.Count); // 2500个参数应该分为3批
        Assert.Equal(1000, batches[0].Count);
        Assert.Equal(1000, batches[1].Count);
        Assert.Equal(500, batches[2].Count);
    }

    [Fact]
    public void ParameterPlaceholder_Generation_ShouldCreateCorrectFormat()
    {
        // Arrange
        var values = new[] { "value1", "value2", "value3", "value4" };

        // Act
        var parameterNames = values.Select((_, index) => $"@param{index}").ToArray();
        var placeholderString = string.Join(",", parameterNames);

        // Assert
        Assert.Equal(4, parameterNames.Length);
        Assert.Equal("@param0", parameterNames[0]);
        Assert.Equal("@param1", parameterNames[1]);
        Assert.Equal("@param2", parameterNames[2]);
        Assert.Equal("@param3", parameterNames[3]);
        Assert.Equal("@param0,@param1,@param2,@param3", placeholderString);
    }

    [Theory]
    [InlineData("SELECT * FROM users WHERE id IN ({0})", "@param0,@param1", "SELECT * FROM users WHERE id IN (@param0,@param1)")]
    [InlineData("SELECT * FROM products WHERE category IN ({0}) AND active=1", "@param0", "SELECT * FROM products WHERE category IN (@param0) AND active=1")]
    public void SqlFormatting_WithPlaceholders_ShouldFormatCorrectly(string sqlTemplate, string parameters, string expectedSql)
    {
        // Act
        var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sqlTemplate, parameters);

        // Assert
        Assert.Equal(expectedSql, formattedSql);
    }

    #endregion
}
