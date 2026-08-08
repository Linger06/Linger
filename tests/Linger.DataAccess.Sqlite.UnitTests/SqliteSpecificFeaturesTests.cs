using System.Data.SQLite;
using Xunit;

namespace Linger.DataAccess.Sqlite.UnitTests;

public sealed class SqliteSpecificFeaturesTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _databasePath;
    private readonly SqliteHelper _helper;

    public SqliteSpecificFeaturesTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"Linger.Sqlite.Features.{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
        _databasePath = Path.Combine(_testDirectory, "features.db");
        _helper = SqliteHelper.CreateFileDatabase(_databasePath);
        _helper.ExecuteBySql("CREATE TABLE users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
        _helper.ExecuteBySql("CREATE TABLE logs (Id INTEGER PRIMARY KEY, Message TEXT NOT NULL)");
        _helper.ExecuteBySql("INSERT INTO users (Id, Name) VALUES (1, 'Alice')");
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
    public void GetDatabaseSize_ShouldReturnPositiveSize()
    {
        var size = _helper.GetDatabaseSize();

        Assert.True(size > 0);
    }

    [Fact]
    public void GetTableNames_ShouldReturnUserTables()
    {
        List<string> names = _helper.GetTableNames();

        Assert.Equal(["logs", "users"], names);
    }

    [Fact]
    public async Task GetTableNamesAsync_ShouldReturnUserTables()
    {
        List<string> names = await _helper.GetTableNamesAsync();

        Assert.Equal(["logs", "users"], names);
    }

    [Theory]
    [InlineData("users", true)]
    [InlineData("USERS", true)]
    [InlineData("missing", false)]
    public void TableExists_ShouldReturnExpectedResult(string tableName, bool expected)
    {
        Assert.Equal(expected, _helper.TableExists(tableName));
    }

    [Fact]
    public async Task TableExistsAsync_WithExistingTable_ShouldReturnTrue()
    {
        Assert.True(await _helper.TableExistsAsync("users"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TableExists_WithInvalidTableName_ShouldThrowArgumentException(string? tableName)
    {
        Assert.ThrowsAny<ArgumentException>(() => _helper.TableExists(tableName!));
    }

    [Fact]
    public void BackupDatabase_ShouldCreateReadableCopy()
    {
        var backupPath = Path.Combine(_testDirectory, "backup.db");

        _helper.BackupDatabase(backupPath);

        using var backup = SqliteHelper.CreateFileDatabase(backupPath, createIfNotExists: false);
        Assert.Equal(1, backup.FindCountBySql("SELECT COUNT(*) FROM users"));
        Assert.True(backup.TableExists("logs"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BackupDatabase_WithInvalidPath_ShouldThrowArgumentException(string? backupPath)
    {
        Assert.ThrowsAny<ArgumentException>(() => _helper.BackupDatabase(backupPath!));
    }
}
