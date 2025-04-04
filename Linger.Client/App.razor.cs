using Linger.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Linger.Client;

public partial class App : IDisposable
{
    [Inject] private AppState AppState { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        // 订阅需要重新登录的事件
        AppState.RequireRelogin += HandleRequireRelogin;
        
        base.OnInitialized();
    }

    private void HandleRequireRelogin()
    {
        // 在UI线程上执行导航
        InvokeAsync(() =>
        {
            // 重定向到登录页面
            NavigationManager.NavigateTo("/login", forceLoad: false);
            
            // 可选：显示消息通知
            DialogService.ShowMessageBox(
                "会话已过期", 
                "您的会话已过期，请重新登录。",
                yesText: "确定");
        });
    }

    public void Dispose()
    {
        // 取消订阅，避免内存泄漏
        AppState.RequireRelogin -= HandleRequireRelogin;
    }
}
