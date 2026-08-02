using System.Data;
using System.Data.Common;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class BaseDatabaseBehaviorExecuteNonQueryTests
{
    [Fact]
    public void ExecuteNonQuery_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var attachedConnection = new RecordingDbConnection("Attached-Conn", () => new RecordingDbCommand(3));
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        var affectedRows = database.ExecuteNonQuery(
            attachedTransaction,
            CommandType.Text,
            "UPDATE demo SET value = 1",
            Array.Empty<DbParameter>());

        Assert.Equal(3, affectedRows);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, attachedConnection.CreateCommandCallCount);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var attachedConnection = new RecordingDbConnection("Attached-Conn", () => new RecordingDbCommand(5));
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        var affectedRows = await database.ExecuteNonQueryAsync(
            attachedTransaction,
            CommandType.Text,
            "UPDATE demo SET value = 2",
            Array.Empty<DbParameter>(),
            CancellationToken.None);

        Assert.Equal(5, affectedRows);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, attachedConnection.CreateCommandCallCount);
    }

    [Fact]
    public void ExecuteNonQuery_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = Assert.Throws<ArgumentNullException>(() =>
            database.ExecuteNonQuery(detachedTransaction, CommandType.Text, "UPDATE demo SET value = 1", Array.Empty<DbParameter>()));

        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            database.ExecuteNonQueryAsync(detachedTransaction, CommandType.Text, "UPDATE demo SET value = 2", Array.Empty<DbParameter>(), CancellationToken.None));

        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
    }
}
