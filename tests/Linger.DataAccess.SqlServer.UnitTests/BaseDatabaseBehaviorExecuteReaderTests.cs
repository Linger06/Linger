using System.Data;
using System.Data.Common;
using Moq;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class BaseDatabaseBehaviorExecuteReaderTests
{
    [Fact]
    public void ExecuteReader_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var attachedConnection = new RecordingDbConnection("Attached-Conn");
        RecordingDbCommand? createdCommand = null;

        provider
            .Setup(x => x.CreateCommand())
            .Returns(() =>
            {
                createdCommand = new RecordingDbCommand(0);
                return createdCommand;
            });

        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
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
        provider.Verify(x => x.CreateConnection("Fallback-Conn"), Times.Never);
        provider.Verify(x => x.CreateCommand(), Times.Once);
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteReaderAsync_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var attachedConnection = new RecordingDbConnection("Attached-Conn");
        RecordingDbCommand? createdCommand = null;

        provider
            .Setup(x => x.CreateCommand())
            .Returns(() =>
            {
                createdCommand = new RecordingDbCommand(0);
                return createdCommand;
            });

        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        using var reader = await database.ExecuteReaderAsync(
            attachedTransaction,
            CommandType.Text,
            "SELECT 1",
            Array.Empty<DbParameter>(),
            CancellationToken.None);

        Assert.NotNull(reader);
        Assert.NotNull(createdCommand);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        Assert.Equal(1, createdCommand!.ExecuteReaderCallCount);
        Assert.Equal(CommandBehavior.Default, createdCommand.LastReaderBehavior);
        provider.Verify(x => x.CreateConnection("Fallback-Conn"), Times.Never);
        provider.Verify(x => x.CreateCommand(), Times.Once);
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public void ExecuteReader_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = Assert.Throws<ArgumentNullException>(() =>
            database.ExecuteReader(detachedTransaction, CommandType.Text, "SELECT 1", Array.Empty<DbParameter>()));

        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteReaderAsync_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            database.ExecuteReaderAsync(detachedTransaction, CommandType.Text, "SELECT 1", Array.Empty<DbParameter>(), CancellationToken.None));

        provider.VerifyNoOtherCalls();
    }
}
