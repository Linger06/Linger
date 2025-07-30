using Linger.Helper;
using Xunit;

namespace Linger.DataAccess.Sqlite.UnitTests;

/// <summary>
/// SQLite特有功能的专门测试类
/// </summary>
public class SqliteSpecificFeaturesTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly SqliteHelper _helper;

    public SqliteSpecificFeaturesTests()
    {
        _testDbPath = Path.GetTempFileName();
        File.Delete(_testDbPath);
        _testDbPath = Path.ChangeExtension(_testDbPath, ".db");
        _helper = SqliteHelper.CreateFileDatabase(_testDbPath);

        InitializeTestData();
    }

    private void InitializeTestData()
    {
        _helper.ExecuteBySql(@"
            CREATE TABLE test_table (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                value INTEGER,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            )");

        _helper.ExecuteBySql(@"
            INSERT INTO test_table (name, value) VALUES 
            ('Item1', 100),
            ('Item2', 200),
            ('Item3', 300),
            ('Item4', 400),
            ('Item5', 500)");
    }

    public void Dispose()
    {
        _helper?.Dispose();

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

    #region 数据库维护测试

    [Fact]
    public void VacuumDatabase_ShouldCompactDatabase()
    {
        // Arrange
        // 插入一些数据然后删除，创建空间碎片
        _helper.ExecuteBySql("INSERT INTO test_table (name, value) SELECT 'temp' || id, id * 1000 FROM test_table");
        _helper.ExecuteBySql("DELETE FROM test_table WHERE name LIKE 'temp%'");

        var sizeBefore = _helper.GetDatabaseSize();

        // Act
        var result = _helper.VacuumDatabase();

        // Assert
        Assert.True(result);

        var sizeAfter = _helper.GetDatabaseSize();
        // VACUUM后数据库大小应该不增加（可能减少）
        Assert.True(sizeAfter <= sizeBefore);
    }

    [Fact]
    public async Task VacuumDatabaseAsync_ShouldCompactDatabase()
    {
        // Act
        var result = await _helper.VacuumDatabaseAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VacuumDatabaseAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.VacuumDatabaseAsync(cts.Token));
    }

    [Fact]
    public void AnalyzeDatabase_ShouldUpdateStatistics()
    {
        // Act
        var result = _helper.AnalyzeDatabase();

        // Assert
        Assert.True(result);

        // 验证统计信息是否更新（检查sqlite_stat1表是否存在）
        var statsTables = _helper.Query("SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'sqlite_stat%'");
        Assert.True(statsTables.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public async Task AnalyzeDatabaseAsync_ShouldUpdateStatistics()
    {
        // Act
        var result = await _helper.AnalyzeDatabaseAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AnalyzeDatabaseAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.AnalyzeDatabaseAsync(cts.Token));
    }

    #endregion

    #region 数据库信息测试

    [Fact]
    public void GetDatabaseSize_ShouldReturnPositiveSize()
    {
        // Act
        var size = _helper.GetDatabaseSize();

        // Assert
        Assert.True(size > 0);
        Assert.True(File.Exists(_testDbPath));

        // 验证返回的大小与实际文件大小一致
        var actualFileSize = new FileInfo(_testDbPath).Length;
        Assert.Equal(actualFileSize, size);
    }

    [Fact]
    public async Task GetDatabaseSizeAsync_ShouldReturnPositiveSize()
    {
        // Act
        var size = await _helper.GetDatabaseSizeAsync();

        // Assert
        Assert.True(size > 0);
    }

    [Fact]
    public async Task GetDatabaseSizeAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.GetDatabaseSizeAsync(cts.Token));
    }

    [Fact]
    public void CheckIntegrity_WithHealthyDatabase_ShouldReturnOk()
    {
        // Act
        var result = _helper.CheckIntegrity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.ToLower());
    }

    [Fact]
    public async Task CheckIntegrityAsync_WithHealthyDatabase_ShouldReturnOk()
    {
        // Act
        var result = await _helper.CheckIntegrityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.ToLower());
    }

    [Fact]
    public async Task CheckIntegrityAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.CheckIntegrityAsync(cts.Token));
    }

    #endregion

    #region 表管理测试

    [Fact]
    public void GetTableNames_ShouldReturnAllTables()
    {
        // Act
        var tableNames = _helper.GetTableNames();

        // Assert
        Assert.NotNull(tableNames);
        Assert.Contains("test_table", tableNames);
        Assert.True(tableNames.Count > 0);
    }

    [Fact]
    public async Task GetTableNamesAsync_ShouldReturnAllTables()
    {
        // Act
        var tableNames = await _helper.GetTableNamesAsync();

        // Assert
        Assert.NotNull(tableNames);
        Assert.Contains("test_table", tableNames);
        Assert.True(tableNames.Count > 0);
    }

    [Fact]
    public async Task GetTableNamesAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.GetTableNamesAsync(cts.Token));
    }

    [Theory]
    [InlineData("test_table", true)]
    [InlineData("non_existent_table", false)]
    [InlineData("TEST_TABLE", false)] // SQLite表名区分大小写
    public void TableExists_ShouldReturnCorrectResult(string tableName, bool expectedExists)
    {
        // Act
        var exists = _helper.TableExists(tableName);

        // Assert
        Assert.Equal(expectedExists, exists);
    }

    [Fact]
    public async Task TableExistsAsync_WithExistingTable_ShouldReturnTrue()
    {
        // Act
        var exists = await _helper.TableExistsAsync("test_table");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task TableExistsAsync_WithNonExistingTable_ShouldReturnFalse()
    {
        // Act
        var exists = await _helper.TableExistsAsync("non_existent_table");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task TableExistsAsync_WithCancellation_ShouldSupportCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _helper.TableExistsAsync("test_table", cts.Token));
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
            Assert.Throws<System.ArgumentNullException>(() => _helper.TableExists(tableName!));
        }
        else
        {
            Assert.Throws<System.ArgumentException>(() => _helper.TableExists(tableName));
        }
    }

    #endregion

    #region 备份功能高级测试

    [Fact]
    public async Task BackupDatabase_ShouldCreateIdenticalCopy()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        File.Delete(backupPath);
        backupPath = Path.ChangeExtension(backupPath, ".db");

        try
        {
            // Act
            var result = _helper.BackupDatabase(backupPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(backupPath));

            // 验证备份文件包含相同的数据
            using (var backupHelper = SqliteHelper.CreateFileDatabase(backupPath, false))
            {
                var originalData = _helper.Query("SELECT COUNT(*) as count FROM test_table");
                var backupData = backupHelper.Query("SELECT COUNT(*) as count FROM test_table");

                Assert.Equal(
                    originalData.Tables[0].Rows[0]["count"],
                    backupData.Tables[0].Rows[0]["count"]
                );
            } // backupHelper在这里被释放

            // 确保连接完全释放，避免文件占用
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        finally
        {
            // 连接已释放，使用RetryHelper处理文件删除
            var retryHelper = new RetryHelper(new RetryOptions
            {
                MaxRetryAttempts = 3,
                DelayMilliseconds = 100,
                UseExponentialBackoff = false
            });

            try
            {
                await retryHelper.ExecuteAsync(
                    () =>
                    {
                        if (File.Exists(backupPath))
                        {
                            File.Delete(backupPath);
                        }
                        return Task.CompletedTask;
                    },
                    "删除备份文件",
                    ex => ex is IOException // 仅对文件IO异常重试
                );
            }
            catch
            {
                // 测试清理失败不应影响测试结果
            }
        }
    }

    [Fact]
    public void BackupDatabase_ToExistingFile_ShouldOverwrite()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        File.WriteAllText(backupPath, "existing content");
        backupPath = Path.ChangeExtension(backupPath, ".db");

        try
        {
            // Act
            var result = _helper.BackupDatabase(backupPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(backupPath));

            // 验证文件不再包含原始内容
            var content = File.ReadAllText(backupPath);
            Assert.DoesNotContain("existing content", content);
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task BackupDatabaseAsync_WithProgressMonitoring_ShouldComplete()
    {
        // Arrange
        var backupPath = Path.GetTempFileName();
        File.Delete(backupPath);
        backupPath = Path.ChangeExtension(backupPath, ".db");

        try
        {
            // Act
            var result = await _helper.BackupDatabaseAsync(backupPath);

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

    #endregion

    #region 事务处理高级测试

    [Fact]
    public void ExecuteInTransaction_WithMixedOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var statements = new[]
        {
            "INSERT INTO test_table (name, value) VALUES ('TransactionTest1', 1000)",
            "UPDATE test_table SET value = value + 100 WHERE name = 'Item1'",
            "DELETE FROM test_table WHERE name = 'Item5'",
            "INSERT INTO test_table (name, value) VALUES ('TransactionTest2', 2000)"
        };

        var originalCount = GetTestTableCount();

        // Act
        var result = _helper.ExecuteInTransaction(statements);

        // Assert
        Assert.True(result);

        // 验证事务执行结果
        var newCount = GetTestTableCount();
        Assert.Equal(originalCount + 1, newCount); // +2 inserts, -1 delete = +1

        // 验证具体的更改
        var item1Value = _helper.Query("SELECT value FROM test_table WHERE name = 'Item1'");
        Assert.Equal(200, Convert.ToInt32(item1Value.Tables[0].Rows[0]["value"])); // 100 + 100

        var item5Exists = _helper.Exists("SELECT COUNT(*) FROM test_table WHERE name = 'Item5'");
        Assert.False(item5Exists);

        var transactionTestExists = _helper.Exists("SELECT COUNT(*) FROM test_table WHERE name LIKE 'TransactionTest%'");
        Assert.True(transactionTestExists);
    }

    [Fact]
    public void ExecuteInTransaction_WithFailingStatement_ShouldRollback()
    {
        // Arrange
        var statements = new[]
        {
            "INSERT INTO test_table (name, value) VALUES ('BeforeError', 9999)",
            "INVALID SQL STATEMENT THAT WILL FAIL",
            "INSERT INTO test_table (name, value) VALUES ('AfterError', 8888)"
        };

        var originalCount = GetTestTableCount();

        // Act
        var result = _helper.ExecuteInTransaction(statements);

        // Assert
        Assert.False(result); // 应该失败

        // 验证回滚 - 数据应该没有变化
        var newCount = GetTestTableCount();
        Assert.Equal(originalCount, newCount);

        // 验证没有插入任何数据
        var beforeErrorExists = _helper.Exists("SELECT COUNT(*) FROM test_table WHERE name = 'BeforeError'");
        Assert.False(beforeErrorExists);

        var afterErrorExists = _helper.Exists("SELECT COUNT(*) FROM test_table WHERE name = 'AfterError'");
        Assert.False(afterErrorExists);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithLargeOperations_ShouldComplete()
    {
        // Arrange
        var statements = new List<string>();
        for (int i = 1; i <= 100; i++)
        {
            statements.Add($"INSERT INTO test_table (name, value) VALUES ('Bulk{i}', {i * 10})");
        }

        var originalCount = GetTestTableCount();

        // Act
        var result = await _helper.ExecuteInTransactionAsync(statements.ToArray());

        // Assert
        Assert.True(result);

        var newCount = GetTestTableCount();
        Assert.Equal(originalCount + 100, newCount);

        // 验证批量插入的数据
        var bulkCount = _helper.Query("SELECT COUNT(*) as count FROM test_table WHERE name LIKE 'Bulk%'");
        Assert.Equal(100, Convert.ToInt32(bulkCount.Tables[0].Rows[0]["count"]));
    }

    private int GetTestTableCount()
    {
        var result = _helper.Query("SELECT COUNT(*) as count FROM test_table");
        return Convert.ToInt32(result.Tables[0].Rows[0]["count"]);
    }

    #endregion

    #region 性能和压力测试

    [Fact]
    public void PerformanceTest_LargeInsertOperations_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var statements = new List<string>();

        for (int i = 1; i <= 1000; i++)
        {
            statements.Add($"INSERT INTO test_table (name, value) VALUES ('Perf{i}', {i})");
        }

        // Act
        var result = _helper.ExecuteInTransaction(statements.ToArray());
        stopwatch.Stop();

        // Assert
        Assert.True(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000); // 应该在5秒内完成

        Console.WriteLine($"插入1000条记录耗时: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void StressTest_MultipleOperations_ShouldMaintainIntegrity()
    {
        // Arrange & Act
        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                var statements = new[]
                {
                    $"INSERT INTO test_table (name, value) VALUES ('Stress{taskId}', {taskId * 100})",
                    $"UPDATE test_table SET value = value + {taskId} WHERE id = 1"
                };
                return await _helper.ExecuteInTransactionAsync(statements);
            }));
        }

        var results = Task.WhenAll(tasks).Result;

        // Assert
        Assert.All(results, result => Assert.True(result));

        // 验证数据完整性
        var stressRecords = _helper.Query("SELECT COUNT(*) as count FROM test_table WHERE name LIKE 'Stress%'");
        Assert.Equal(10, Convert.ToInt32(stressRecords.Tables[0].Rows[0]["count"]));
    }

    #endregion
}
