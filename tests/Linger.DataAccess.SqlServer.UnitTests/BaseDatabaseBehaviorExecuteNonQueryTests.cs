using System.Data;
using System.Data.Common;
using Moq;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class BaseDatabaseBehaviorExecuteNonQueryTests
{
    [Fact]
    public void ExecuteNonQuery_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var attachedConnection = new RecordingDbConnection("Attached-Conn");

        provider
            .Setup(x => x.CreateCommand())
            .Returns(() => new RecordingDbCommand(3));

        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        var affectedRows = database.ExecuteNonQuery(
            attachedTransaction,
            CommandType.Text,
            "UPDATE demo SET value = 1",
            Array.Empty<DbParameter>());

        Assert.Equal(3, affectedRows);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        provider.Verify(x => x.CreateConnection("Fallback-Conn"), Times.Never);
        provider.Verify(x => x.CreateCommand(), Times.Once);
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithAttachedTransaction_ShouldUseTransactionConnection()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var attachedConnection = new RecordingDbConnection("Attached-Conn");

        provider
            .Setup(x => x.CreateCommand())
            .Returns(() => new RecordingDbCommand(5));

        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var attachedTransaction = BaseDatabaseBehaviorTestSupport.CreateAttachedTransaction(attachedConnection);

        var affectedRows = await database.ExecuteNonQueryAsync(
            attachedTransaction,
            CommandType.Text,
            "UPDATE demo SET value = 2",
            Array.Empty<DbParameter>(),
            CancellationToken.None);

        Assert.Equal(5, affectedRows);
        Assert.Equal(1, attachedConnection.OpenCallCount);
        provider.Verify(x => x.CreateConnection("Fallback-Conn"), Times.Never);
        provider.Verify(x => x.CreateCommand(), Times.Once);
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public void ExecuteNonQuery_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = Assert.Throws<ArgumentNullException>(() =>
            database.ExecuteNonQuery(detachedTransaction, CommandType.Text, "UPDATE demo SET value = 1", Array.Empty<DbParameter>()));

        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithDetachedTransaction_ShouldThrowArgumentNullException()
    {
        var provider = new Mock<IProvider>(MockBehavior.Strict);
        var database = new TestableBaseDatabase(provider.Object, "Fallback-Conn");
        var detachedTransaction = BaseDatabaseBehaviorTestSupport.CreateDetachedTransaction();

        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            database.ExecuteNonQueryAsync(detachedTransaction, CommandType.Text, "UPDATE demo SET value = 2", Array.Empty<DbParameter>(), CancellationToken.None));

        provider.VerifyNoOtherCalls();
    }
}
