using System.Collections;
using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;

namespace Linger.DataAccess.SqlServer.UnitTests;

internal static class BaseDatabaseBehaviorTestSupport
{
    internal static DbTransaction CreateAttachedTransaction(DbConnection connection)
    {
        return new AttachedDbTransaction(connection, IsolationLevel.ReadCommitted);
    }

    internal static DbTransaction CreateDetachedTransaction()
    {
        var transaction = new Mock<DbTransaction>(MockBehavior.Strict);
        transaction.SetupGet(x => x.IsolationLevel).Returns(IsolationLevel.ReadCommitted);
        transaction.Setup(x => x.Commit());
        transaction.Setup(x => x.Rollback());
        transaction.Protected().Setup<DbConnection?>("DbConnection").Returns((DbConnection?)null);
        return transaction.Object;
    }
}

internal sealed class TestableBaseDatabase : BaseDatabase
{
    public TestableBaseDatabase(IProvider provider, string connectionString)
        : base(provider, connectionString)
    {
    }
}

internal sealed class RecordingDbConnection : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;

    public RecordingDbConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public int OpenCallCount { get; private set; }

    public override string ConnectionString { get; set; }

    public override string Database => "RecordingDb";

    public override string DataSource => "RecordingSource";

    public override string ServerVersion => "1.0";

    public override ConnectionState State => _state;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    public override void Open()
    {
        OpenCallCount++;
        _state = ConnectionState.Open;
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Open();
        return Task.CompletedTask;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new AttachedDbTransaction(this, isolationLevel);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new RecordingDbCommand(0) { Connection = this };
    }
}

internal sealed class AttachedDbTransaction : DbTransaction
{
    public AttachedDbTransaction(DbConnection connection, IsolationLevel isolationLevel)
    {
        DbConnection = connection;
        IsolationLevel = isolationLevel;
    }

    public override IsolationLevel IsolationLevel { get; }

    protected override DbConnection DbConnection { get; }

    public override void Commit()
    {
    }

    public override void Rollback()
    {
    }
}

internal sealed class RecordingDbCommand : DbCommand
{
    private readonly RecordingDbParameterCollection _parameters = new();
    private readonly int _nonQueryResult;

    public RecordingDbCommand(int nonQueryResult)
    {
        _nonQueryResult = nonQueryResult;
    }

    public override string CommandText { get; set; } = string.Empty;

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; }

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    public int ExecuteReaderCallCount { get; private set; }

    public CommandBehavior? LastReaderBehavior { get; private set; }

    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection => _parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
    }

    public override int ExecuteNonQuery()
    {
        return _nonQueryResult;
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_nonQueryResult);
    }

    public override object? ExecuteScalar()
    {
        return null;
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<object?>(null);
    }

    public override void Prepare()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new RecordingDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        ExecuteReaderCallCount++;
        LastReaderBehavior = behavior;
        var table = new DataTable();
        table.Columns.Add("Value", typeof(int));
        return table.CreateDataReader();
    }
}

internal sealed class RecordingDbParameter : DbParameter
{
    public override DbType DbType { get; set; }

    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public override bool IsNullable { get; set; }

    public override string ParameterName { get; set; } = string.Empty;

    public override string SourceColumn { get; set; } = string.Empty;

    public override object? Value { get; set; }

    public override bool SourceColumnNullMapping { get; set; }

    public override int Size { get; set; }

    public override void ResetDbType()
    {
    }
}

internal sealed class RecordingDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _items = new();

    public override int Count => _items.Count;

    public override object SyncRoot => ((ICollection)_items).SyncRoot;

    public override int Add(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is not DbParameter parameter)
        {
            throw new ArgumentException("Value must be a DbParameter.", nameof(value));
        }

        _items.Add(parameter);
        return _items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var value in values)
        {
            _ = Add(value!);
        }
    }

    public override void Clear()
    {
        _items.Clear();
    }

    public override bool Contains(object value)
    {
        return value is DbParameter parameter && _items.Contains(parameter);
    }

    public override bool Contains(string value)
    {
        return IndexOf(value) >= 0;
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)_items).CopyTo(array, index);
    }

    public override IEnumerator GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    protected override DbParameter GetParameter(int index)
    {
        return _items[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        var index = IndexOf(parameterName);
        if (index < 0)
        {
            throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
        }

        return _items[index];
    }

    public override int IndexOf(object value)
    {
        return value is DbParameter parameter ? _items.IndexOf(parameter) : -1;
    }

    public override int IndexOf(string parameterName)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].ParameterName, parameterName, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    public override void Insert(int index, object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is not DbParameter parameter)
        {
            throw new ArgumentException("Value must be a DbParameter.", nameof(value));
        }

        _items.Insert(index, parameter);
    }

    public override void Remove(object value)
    {
        if (value is DbParameter parameter)
        {
            _items.Remove(parameter);
        }
    }

    public override void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            _items.RemoveAt(index);
        }
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        _items[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var index = IndexOf(parameterName);
        if (index < 0)
        {
            _items.Add(value);
            return;
        }

        _items[index] = value;
    }
}
