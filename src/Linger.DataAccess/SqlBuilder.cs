using System.Globalization;
using System.Text;

namespace Linger.DataAccess;

public class SqlBuilder(ParameterList paraList)
{
    private readonly StringBuilder _sb = new();

    public void Append(string sql)
    {
        _ = _sb.Append(sql);
    }

    public void AppendFormat<T>(string sql, string key)
    {
        if (paraList.ContainsKey(key))
        {
            _ = _sb.AppendFormat(CultureInfo.InvariantCulture, sql, paraList.Get<T>(key));
        }
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}
