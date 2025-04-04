using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Resilience;
using Polly;

namespace Linger.Client.Services;

/// <summary>
/// 令牌刷新处理器 - 配合Microsoft.Extensions.Http.Resilience使用
/// </summary>
public class TokenRefreshHandler
{
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TokenRefreshHandler(AppState appState, IServiceProvider serviceProvider)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 配置令牌刷新弹性管道
    /// </summary>
    public void ConfigureTokenRefreshResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        // 添加处理401(Unauthorized)的弹性策略
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            // 设置最大重试次数为1（只尝试刷新令牌一次）
            MaxRetryAttempts = 1,
            // 只有401错误才触发令牌刷新
            ShouldHandle = args => 
            {
                bool shouldRetry = args.Outcome.Result?.StatusCode == HttpStatusCode.Unauthorized;
                return ValueTask.FromResult(shouldRetry);
            },
            // 在重试前执行令牌刷新
            OnRetry = async context =>
            {
                // 使用信号量防止多个请求同时尝试刷新令牌
                await _semaphore.WaitAsync();
                try
                {
                    // 尝试刷新令牌
                    await RefreshTokenAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            // 重试延迟设为0，令牌刷新后立即重试
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.Zero
        });
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    private async Task RefreshTokenAsync()
    {
        try
        {
            // 获取认证服务来刷新令牌
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            // 使用当前令牌获取新的令牌
            var (success, newToken) = await authService.RefreshTokenAsync(_appState.Token, string.Empty);

            if (success && !string.IsNullOrEmpty(newToken))
            {
                // 更新AppState中的令牌
                _appState.Token = newToken;
                // Token属性setter会自动通知变更
            }
            else
            {
                // 如果刷新失败，清除令牌
                _appState.Token = string.Empty;
                // 触发需要重新登录的事件
                _appState.RaiseRequireReloginEvent();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"令牌刷新失败: {ex.Message}");
            
            // 清除无效令牌
            _appState.Token = string.Empty;
            
            // 触发重新登录事件
            _appState.RaiseRequireReloginEvent();
        }
    }
}
