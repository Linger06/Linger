using System.Data;
using System.Data.Common;
using System.Globalization;

namespace Linger.DataAccess;

/// <summary>
/// 把 <see cref="DbDataReader"/> 的全部结果集填充到 <see cref="DataSet"/>。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DataAdapter"/> 是抽象类但没有抽象成员，其 protected 的
/// <c>Fill(DataSet, string, IDataReader, int, int)</c> 就是 <c>DataAdapter.Fill(DataSet)</c> 内部走的那条路径。
/// 继承它即可复用 BCL 官方的表命名（Table、Table1…）、重名列去重（Id、Id1…）、无类型列退化为
/// <see cref="object"/> 等全部规则，不必手写去猜这些细节；同时不需要 provider 提供具体的
/// <c>DataAdapter</c>（那要求向下转型到 <c>SqlCommand</c> 之类，跨 provider 会运行时炸）。
/// </para>
/// <para>
/// 只有同步版本：BCL 没有异步填充 API，异步场景请用 <c>ExecuteReaderAsync</c> 自行逐行读取。
/// </para>
/// </remarks>
internal sealed class DataSetReader : DataAdapter
{
    private DataSetReader()
    {
    }

    internal static DataSet Read(DbDataReader reader)
    {
        // Locale 设在 DataSet 上即可，其下的表未显式设置时跟随 DataSet
        var dataSet = new DataSet { Locale = CultureInfo.InvariantCulture };

        using var adapter = new DataSetReader();
        _ = adapter.Fill(dataSet, "Table", reader, 0, 0);

        return dataSet;
    }
}
