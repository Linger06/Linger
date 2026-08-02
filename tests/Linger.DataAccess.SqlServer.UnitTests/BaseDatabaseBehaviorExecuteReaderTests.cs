using System.Data;
using System.Data.Common;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class BaseDatabaseBehaviorExecuteReaderTests
{
    [Fact]
    public void ExecuteReader_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        RecordingDbCommand? createdCommand = null;
        var attachedConnection = new RecordingDbConnection("Attached-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        using var reader = database.ExecuteReader(
            attachedTransaction,
            CommandType.Text,
            "SELECT 1",
            Array.Empty<DbParameter>());

        Assert.NotNull(reader);
        Assert.NotNull(createdCommand);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        Assert.Equal(1, createdCommand!.ExecuteReaderCallCount);
        Assert.Equal(CommandBehavior.Default, createdCommand.LastReaderBehavior);
        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, attachedConnection.CreateCommandCallCount);
    }

    [Fact]
    public async Task ExecuteReaderAsync_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        RecordingDbCommand? createdCommand = null;
        var attachedConnection = new RecordingDbConnection("Attached-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        using var cancellation = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellation.Token;
        using var reader = await database.ExecuteReaderAsync(
            attachedTransaction,
            CommandType.Text,
            "SELECT 1",
            Array.Empty<DbParameter>(),
            cancellationToken);

        Assert.NotNull(reader);
        Assert.NotNull(createdCommand);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        Assert.Equal(1, createdCommand!.ExecuteReaderCallCount);
        Assert.Equal(CommandBehavior.Default, createdCommand.LastReaderBehavior);
        Assert.Equal(cancellationToken, attachedConnection.LastOpenCancellationToken);
        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, attachedConnection.CreateCommandCallCount);
    }

    [Fact]
    public void ExecuteReader_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = Assert.Throws<ArgumentNullException>(() =>
            database.ExecuteReader(detachedTransaction, CommandType.Text, "SELECT 1", Array.Empty<DbParameter>()));

        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
    }

    [Fact]
    public async Task ExecuteReaderAsync_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var factory = new RecordingDbProviderFactory();
        var database = new TestableBaseDatabase(factory, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            database.ExecuteReaderAsync(detachedTransaction, CommandType.Text, "SELECT 1", Array.Empty<DbParameter>(), CancellationToken.None));

        Assert.Equal(0, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
    }
}
