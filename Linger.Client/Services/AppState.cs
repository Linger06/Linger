namespace Linger.Client.Services;

/// <summary>
/// 应用状态管理类，用于存储跨组件的应用状态
/// </summary>
public class AppState
{
    private bool _isLoggedIn;
    private string _username = string.Empty;
    private string _token = string.Empty;

    /// <summary>
    /// 用户的JWT认证令牌
    /// </summary>
    public string? Token
    {
        get => _token;
        set
        {
            _token = value ?? string.Empty;
            NotifyStateChanged();
        }
    }

    // 添加刷新令牌属性
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 用户是否已登录
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            NotifyStateChanged();
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    // 可选：添加需要重新登录的事件
    public event Action? RequireRelogin;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void RaiseRequireReloginEvent()
    {
        RequireRelogin?.Invoke();
    }
}
