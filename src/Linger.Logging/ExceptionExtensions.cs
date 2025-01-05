using Linger.Extensions.Exception;

namespace Linger.Logging;

/// <summary>
///     <see cref="Exception" /> 扩展
/// </summary>
public static class ExceptionExtensions
{
    #region 将当前 Exception 对象写入日志

    /// <summary>
    ///     将当前 <see cref="Exception" /> 对象写入日志
    /// </summary>
    /// <param name="exception">当前 Exception 对象</param>
    public static void WriteLog(this Exception exception)
    {
        LogHelper.Error(exception.ExtractAllStackTrace());
    }

    #endregion 将当前 Exception 对象写入日志
}