using System.Data;
using Xunit;

namespace Linger.DataAccess.SqlServer.UnitTests;

public class BaseDatabaseBehaviorExecuteReaderTests
{
    [Fact]
    public void IBaseDatabase_ShouldNotExposeRawReaderMethods()
    {
        Assert.DoesNotContain(typeof(IBaseDatabase).GetMethods(),
            static method => method.Name is "ExecuteReader" or "ExecuteReaderAsync");
    }

    [Fact]
    public void GetDataSet_WhenCompleted_ShouldDisposeReaderCommandAndOwnedConnection()
    {
        RecordingDbCommand? createdCommand = null;
        var connection = new RecordingDbConnection("Reader-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new TestableBaseDatabase(factory, "Reader-Conn");
        var parameter = new RecordingDbParameter { ParameterName = "@id", Value = 1 };

        DataSet result = database.GetDataSet(CommandType.Text, "SELECT 1", parameter);

        Assert.NotNull(result);
        Assert.NotNull(createdCommand);
        Assert.True(createdCommand.IsDisposed);
        Assert.Equal(0, createdCommand.ParameterCount);
        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task HasRowsAsync_WhenCompleted_ShouldDisposeReaderCommandAndOwnedConnection()
    {
        RecordingDbCommand? createdCommand = null;
        var connection = new RecordingDbConnection("Reader-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new Database(factory, "Reader-Conn");
        var parameter = new RecordingDbParameter { ParameterName = "@id", Value = 1 };

        bool result = await database.HasRowsAsync("SELECT 1", [parameter], CancellationToken.None);

        Assert.False(result);
        Assert.NotNull(createdCommand);
        Assert.True(createdCommand.IsDisposed);
        Assert.Equal(0, createdCommand.ParameterCount);
        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public void GetDataSet_WhenCommandCreationFails_ShouldDisposeOwnedConnection()
    {
        var connection = new RecordingDbConnection("Reader-Conn",
            () => throw new InvalidOperationException("Command creation failed."));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new TestableBaseDatabase(factory, "Reader-Conn");

        _ = Assert.Throws<InvalidOperationException>(() =>
            database.GetDataSet(CommandType.Text, "SELECT 1"));

        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task HasRowsAsync_WhenCommandCreationFails_ShouldDisposeOwnedConnection()
    {
        var connection = new RecordingDbConnection("Reader-Conn",
            () => throw new InvalidOperationException("Command creation failed."));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new Database(factory, "Reader-Conn");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            database.HasRowsAsync("SELECT 1"));

        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public void GetDataSet_InAmbientTransaction_ShouldKeepConnectionUntilRollback()
    {
        RecordingDbCommand? createdCommand = null;
        var connection = new RecordingDbConnection("Transaction-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new TestableBaseDatabase(factory, "Transaction-Conn");

        _ = database.BeginTrans();
        DataSet result = database.GetDataSet(CommandType.Text, "SELECT 1");

        Assert.NotNull(result);
        Assert.NotNull(createdCommand);
        Assert.Equal(1, connection.OpenCallCount);
        Assert.Equal(1, createdCommand!.ExecuteReaderCallCount);
        Assert.Equal(CommandBehavior.Default, createdCommand.LastReaderBehavior);
        Assert.Equal(1, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, connection.CreateCommandCallCount);
        Assert.True(createdCommand.IsDisposed);
        Assert.False(connection.IsDisposed);
        Assert.Equal(ConnectionState.Open, connection.State);

        database.Rollback();

        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task HasRowsAsync_InAmbientTransaction_ShouldKeepConnectionUntilRollback()
    {
        RecordingDbCommand? createdCommand = null;
        var connection = new RecordingDbConnection("Transaction-Conn",
            () => createdCommand = new RecordingDbCommand(0));
        var factory = new RecordingDbProviderFactory(createConnection: () => connection);
        var database = new Database(factory, "Transaction-Conn");

        using var cancellation = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellation.Token;
        _ = await database.BeginTransAsync(cancellationToken);
        bool result = await database.HasRowsAsync("SELECT 1", cancellationToken: cancellationToken);

        Assert.False(result);
        Assert.NotNull(createdCommand);
        Assert.Equal(1, connection.OpenCallCount);
        Assert.Equal(1, createdCommand!.ExecuteReaderCallCount);
        Assert.Equal(CommandBehavior.Default, createdCommand.LastReaderBehavior);
        Assert.Equal(cancellationToken, connection.LastOpenCancellationToken);
        Assert.Equal(1, factory.CreateConnectionCallCount);
        Assert.Equal(0, factory.CreateCommandCallCount);
        Assert.Equal(1, connection.CreateCommandCallCount);
        Assert.True(createdCommand.IsDisposed);
        Assert.False(connection.IsDisposed);
        Assert.Equal(ConnectionState.Open, connection.State);

        await database.RollbackAsync(cancellationToken);

        Assert.True(connection.IsDisposed);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }
}
