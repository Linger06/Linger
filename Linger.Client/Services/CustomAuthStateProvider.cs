using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Linger.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AppState _appState;

    public CustomAuthStateProvider(AppState appState)
    {
        _appState = appState;
        _appState.OnChange += NotifyAuthenticationStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_appState.IsLoggedIn)
        {
            // 未登录状态返回空身份
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        // 已登录状态创建包含用户信息的身份
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, _appState.Username),
        }, "Custom Authentication");

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    public void Dispose()
    {
        _appState.OnChange -= NotifyAuthenticationStateChanged;
    }

    // 添加此方法到类中
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
