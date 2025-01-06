namespace Linger.Logging.Abstractions;

/// <summary>
/// 日志提供程序接口
/// </summary>
public interface ILingerLoggerProvider : IDisposable
{
    /// <summary>
    /// 获取指定类别的日志记录器
    /// </summary>
    ILingerLogger GetLogger(string categoryName);
}
