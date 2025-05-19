using Linger.Blazor.Models;
using Linger.HttpClient.Contracts.Core;
using Microsoft.Extensions.Logging;

namespace Linger.Blazor.Services;

/// <summary>
/// 认证服务，使用IHttpClient处理登录、注销
/// </summary>
public class AuthService(IHttpClient httpClient, AppState appState, ILogger<AuthService>? logger = null)
{
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
            logger?.LogInformation("尝试登录用户: {LoginRequestUsername}", loginRequest.Username);

            // 直接使用IHttpClient发送POST请求
            var result = await httpClient.CallApi<LoginResponse>(
                "api/auth/login",
                HttpMethodEnum.Post,
                postData: loginRequest,
                cancellationToken: cancellationToken);

            if (!result.IsSuccess)
            {
                logger?.LogWarning("登录失败: {ResultErrorMsg}", result.ErrorMsg);
                return false;
            }

            // 保存令牌和用户信息到应用状态
            appState.Token = result.Data.Token;
            appState.Username = loginRequest.Username;
            appState.IsLoggedIn = true;

            // 设置令牌到HttpClient
            httpClient.SetToken(result.Data.Token);

            logger?.LogInformation("用户 {LoginRequestUsername} 登录成功", loginRequest.Username);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "登录过程中发生异常: {ExMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 注销方法
    /// </summary>
    public Task<bool> Logout()
    {
        logger?.LogInformation("用户 {AppStateUsername} 注销", appState.Username);

        // 清除令牌和用户信息
        appState.Token = string.Empty;
        appState.Username = string.Empty;
        appState.IsLoggedIn = false;

        // 从HttpClient中移除令牌
        httpClient.SetToken(string.Empty);

        return Task.FromResult(true);
    }

    /// <summary>
    /// 检查当前是否已认证
    /// </summary>
    public bool IsAuthenticated => appState.IsAuthenticated;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public async Task<(bool success, string newToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            // 创建刷新令牌请求的数据
            var refreshRequest = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            // 调用刷新令牌API
            var response = await httpClient.CallApi<TokenResponse>(
                "api/auth/refresh",
                HttpMethodEnum.Post,
                refreshRequest);

            if (response.IsSuccess && response.Data != null)
            {
                return (true, response.Data.AccessToken);
            }

            return (false, string.Empty);
        }
        catch
        {
            return (false, string.Empty);
        }
    }

    // 令牌响应模型
    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}