using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
/// 表示一条待执行的参数化 SQL 语句。
/// </summary>
/// <remarks>
/// 参数对象的所有权仍属于调用方；执行完成后可读取输出参数或复用参数对象。
/// 无参数语句可以省略构造函数的参数数组。
/// </remarks>
/// <example>
/// <code>
/// var statement = new SqlStatement("DELETE FROM TempData");
/// </code>
/// </example>
public sealed class SqlStatement
{
    /// <summary>
    /// 初始化一条参数化 SQL 语句。
    /// </summary>
    /// <param name="sql">要执行的 SQL 文本。</param>
    /// <param name="parameters">SQL 参数；无参数时可省略。</param>
    /// <exception cref="ArgumentException"><paramref name="sql"/> 为空或全为空白时抛出。</exception>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> 为 null 时抛出。</exception>
    public SqlStatement(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        Sql = sql;
        Parameters = parameters;
    }

    /// <summary>
    /// 获取 SQL 文本。
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// 获取 SQL 参数；无参数语句返回空数组。
    /// </summary>
    public DbParameter[] Parameters { get; }
}
