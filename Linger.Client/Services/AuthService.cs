using Linger.Client.Models;
using Linger.HttpClient.Contracts.Core;
using Microsoft.Extensions.Logging;

namespace Linger.Client.Services;

/// <summary>
/// 认证服务，使用IHttpClient处理登录、注销
/// </summary>
public class AuthService
{
    private readonly IHttpClient _httpClient;
    private readonly AppState _appState;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(IHttpClient httpClient, AppState appState, ILogger<AuthService>? logger = null)
    {
        _httpClient = httpClient;
        _appState = appState;
        _logger = logger;
    }

    /// <summary>
    /// 登录方法
    /// </summary>
    /// <param name="loginRequest">登录请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录成功返回true，否则返回false</returns>
    public async Task<bool> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation($"尝试登录用户: {loginRequest.Username}");

            // 直接使用IHttpClient发送POST请求
            var result = await _httpClient.CallApi<LoginResponse>(
                "api/auth/login",
                HttpMethodEnum.Post,
                postData: loginRequest,
                cancellationToken: cancellationToken);

            if (!result.IsSuccess)
            {
                _logger?.LogWarning($"登录失败: {result.ErrorMsg}");
                return false;
            }

            // 保存令牌和用户信息到应用状态
            _appState.Token = result.Data.Token;
            _appState.Username = loginRequest.Username;
            _appState.IsLoggedIn = true;

            // 设置令牌到HttpClient
            _httpClient.SetToken(result.Data.Token);

            _logger?.LogInformation($"用户 {loginRequest.Username} 登录成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"登录过程中发生异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 注销方法
    /// </summary>
    public Task<bool> Logout()
    {
        _logger?.LogInformation($"用户 {_appState.Username} 注销");

        // 清除令牌和用户信息
        _appState.Token = null;
        _appState.Username = string.Empty;
        _appState.IsLoggedIn = false;

        // 从HttpClient中移除令牌
        _httpClient.SetToken(string.Empty);

        return Task.FromResult(true);
    }

    /// <summary>
    /// 检查当前是否已认证
    /// </summary>
    public bool IsAuthenticated => _appState.IsAuthenticated;
}

/// <summary>
/// 登录响应模型
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
