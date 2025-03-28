using Linger.Client.Models;
using Linger.HttpClient.Contracts;
using Microsoft.Extensions.Logging;

namespace Linger.Client.Services
{
    /// <summary>
    /// 认证服务，使用IHttpClient处理登录、注销和令牌刷新
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
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
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
        /// 刷新令牌
        /// </summary>
        /// <returns>刷新成功返回true，否则返回false</returns>
        public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(_appState.Token))
                {
                    _logger?.LogWarning("无法刷新令牌，当前没有有效的令牌");
                    return false;
                }

                _logger?.LogInformation("尝试刷新令牌");

                // 创建刷新令牌请求数据
                var refreshData = new { Token = _appState.Token };

                // 发送刷新令牌请求
                var result = await _httpClient.CallApi<LoginResponse>(
                    "api/auth/refresh",
                    HttpMethodEnum.Post,
                    postData: refreshData,
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                {
                    _logger?.LogWarning($"刷新令牌失败: {result.ErrorMsg}");
                    return false;
                }

                // 更新令牌
                _appState.Token = result.Data.Token;

                // 设置新令牌到HttpClient
                _httpClient.SetToken(result.Data.Token);

                _logger?.LogInformation("令牌刷新成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"刷新令牌过程中发生异常: {ex.Message}");
                return false;
            }
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
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
