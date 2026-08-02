using Oracle.ManagedDataAccess.Client;

namespace Linger.DataAccess.Oracle;

/// <summary>
/// Oracle 数据库帮助类。
/// </summary>
/// <param name="connectionString">数据库连接字符串</param>
/// <remarks>
/// <para>
/// 查询、执行、事务、存在性检查与批量事务均由 <see cref="Database"/> 提供。
/// 由于 <c>OracleParameter[]</c> 可协变为 <c>DbParameter[]</c>，
/// 直接把 <see cref="OracleParameter"/> 传给基类方法即可，本类不再重复声明参数重载。
/// </para>
/// <para>本类仅覆盖 Oracle 特有的参数前缀（<c>:</c>）。</para>
/// </remarks>
/// <example>
/// <code>
/// using var helper = new OracleHelper(connectionString);
///
/// var exists = helper.HasRows("SELECT 1 FROM USERS WHERE ID = :id", new OracleParameter(":id", 1));
/// var ds = helper.Query("SELECT * FROM USERS WHERE DEPT = :dept", new OracleParameter(":dept", "IT"));
/// </code>
/// </example>
public class OracleHelper(string connectionString) : Database(OracleClientFactory.Instance, connectionString)
{
    /// <summary>
    ///     获取Oracle参数名称（使用: 前缀）
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <returns>参数名称</returns>
    protected override string GetParameterName(int index)
    {
        return $":param{index}";
    }
}
