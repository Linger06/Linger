namespace Linger.Email;

/// <summary>
/// 邮件发送基础接口
/// </summary>
public interface IEmail : IAsyncDisposable
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="emailMessage">邮件消息</param>
    /// <param name="completedCallback">发送完成回调</param>
    /// <returns>发送任务</returns>
    Task SendAsync(EmailMessage emailMessage, Action<string>? completedCallback = null);
}
