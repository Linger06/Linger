using Linger.Blazor.Services;
using Microsoft.JSInterop;

namespace Linger.Client.Services;

/// <summary>
/// WinForms平台的全屏服务实现
/// </summary>
public class WinFormFullScreenService : IFullScreenService, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private Action<bool>? _fullScreenCallback;
    private DotNetObjectReference<WinFormFullScreenService>? _objRef;

    // 静态实例用于JSInvokable方法访问
    private static WinFormFullScreenService? s_instance;

    public WinFormFullScreenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        s_instance = this;
    }

    /// <summary>
    /// 获取当前是否处于全屏状态
    /// </summary>
    public async Task<bool> IsFullScreenAsync()
    {
        return await _jsRuntime.InvokeAsync<bool>("getFullScreenState");
    }

    /// <summary>
    /// 切换全屏状态
    /// </summary>
    public async Task ToggleFullScreenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("toggleFullScreenWinForm");
    }

    /// <summary>
    /// 初始化全屏状态监听
    /// </summary>
    public async Task InitializeFullScreenListenerAsync(Action<bool> callback)
    {
        _fullScreenCallback = callback;
        _objRef = DotNetObjectReference.Create(this);

        await _jsRuntime.InvokeVoidAsync("eval", $@"
            window.addEventListener('fullScreenChanged', function (e) {{
                DotNet.invokeMethodAsync('{typeof(WinFormFullScreenService).Assembly.GetName().Name}', 
                                         'ReceiveFullScreenState', 
                                         e.detail);
            }});
        ");
    }

    /// <summary>
    /// 接收来自JavaScript的全屏状态更新
    /// </summary>
    [JSInvokable]
    public static Task ReceiveFullScreenState(bool isFullScreen)
    {
        s_instance?._fullScreenCallback?.Invoke(isFullScreen);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _objRef?.Dispose();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }
}