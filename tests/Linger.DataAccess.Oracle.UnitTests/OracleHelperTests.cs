using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using Xunit;

namespace Linger.DataAccess.Oracle.UnitTests;

public class OracleHelperTests
{
    private const string ConnectionString =
        "User Id=testuser;Password=testpass;Data Source=localhost:1521/XEPDB1;";

    [Fact]
    public void Constructor_WithValidConnectionString_ShouldCreateInstance()
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.NotNull(helper);
        Assert.IsAssignableFrom<IDatabase>(helper);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidConnectionString_ShouldThrowArgumentException(string? connectionString)
    {
        Assert.ThrowsAny<ArgumentException>(() => new OracleHelper(connectionString!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void QueryInBatches_WithInvalidSql_ShouldThrowArgumentException(string? sql)
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.QueryInBatches(sql!, []));
    }

    [Fact]
    public void QueryInBatches_WithNullParameters_ShouldThrowArgumentNullException()
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.Throws<ArgumentNullException>(() => helper.QueryInBatches("SELECT 1 FROM DUAL WHERE 1 IN ({0})", null!));
    }

    [Fact]
    public void QueryInBatches_WithEmptyParameters_ShouldReturnEmptyDataTable()
    {
        var helper = new OracleHelper(ConnectionString);

        var result = helper.QueryInBatches("SELECT 1 FROM DUAL WHERE 1 IN ({0})", []);

        Assert.Empty(result.Rows);
        Assert.Empty(result.Columns);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void QueryInBatches_WithInvalidBatchSize_ShouldThrowArgumentOutOfRangeException(int batchSize)
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            helper.QueryInBatches("SELECT 1 FROM DUAL WHERE 1 IN ({0})", [], batchSize));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasRows_WithInvalidSql_ShouldThrowArgumentException(string? sql)
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.HasRows(sql!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FindCountBySql_WithInvalidSql_ShouldThrowArgumentException(string? sql)
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.ThrowsAny<ArgumentException>(() => helper.FindCountBySql(sql!));
    }

    [Fact]
    public void OracleParameter_ShouldBeAcceptedAsDatabaseParameter()
    {
        var parameter = new OracleParameter(":name", OracleDbType.Varchar2) { Value = "Alice" };

        Assert.Equal(":name", parameter.ParameterName);
        Assert.Equal("Alice", parameter.Value);
    }

    [Fact]
    public void GetParameterName_ShouldUseOraclePrefix()
    {
        var helper = new TestableOracleHelper(ConnectionString);

        Assert.Equal(":param7", helper.GetParameterNameForTest(7));
    }

    [Fact]
    public void ConfigureCommand_ShouldEnableBindingByName()
    {
        var helper = new OracleHelper(ConnectionString);
        using var command = new OracleCommand();
        MethodInfo? method = typeof(OracleHelper).GetMethod(
            "ConfigureCommand",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.False(command.BindByName);
        Assert.NotNull(method);

        _ = method!.Invoke(helper, [command]);

        Assert.True(command.BindByName);
    }

    [Fact]
    public void CommandTimeout_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        var helper = new OracleHelper(ConnectionString);

        Assert.Throws<ArgumentOutOfRangeException>(() => helper.CommandTimeout = -1);
    }

    private sealed class TestableOracleHelper(string connectionString) : OracleHelper(connectionString)
    {
        public string GetParameterNameForTest(int index) => GetParameterName(index);
    }
}
