using Microsoft.JSInterop;

namespace Linger.Blazor.Services;

/// <summary>
/// 基于浏览器原生API的全屏服务实现，适用于 Blazor WebAssembly 和 Blazor Server
/// </summary>
public class BrowserFullScreenService : IFullScreenService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<BrowserFullScreenService>? _objRef;
    private Action<bool>? _fullScreenCallback;

    public BrowserFullScreenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task<IJSObjectReference> GetJsModuleAsync()
    {
        _jsModule ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Linger.Blazor/js/fullScreenService.js");
        return _jsModule;
    }

    /// <summary>
    /// 初始化全屏状态监听
    /// </summary>
    public async Task InitializeFullScreenListenerAsync(Action<bool> callback)
    {
        _fullScreenCallback = callback;
        _objRef = DotNetObjectReference.Create(this);

        var module = await GetJsModuleAsync();
        await module.InvokeVoidAsync("initFullScreenListener", _objRef);
    }

    /// <summary>
    /// 获取当前是否处于全屏状态
    /// </summary>
    public async Task<bool> IsFullScreenAsync()
    {
        var module = await GetJsModuleAsync();
        return await module.InvokeAsync<bool>("isFullScreen");
    }

    /// <summary>
    /// 切换全屏状态
    /// </summary>
    public async Task ToggleFullScreenAsync()
    {
        var module = await GetJsModuleAsync();
        await module.InvokeVoidAsync("toggleFullScreen");
    }

    /// <summary>
    /// 接收来自JavaScript的全屏状态更新
    /// </summary>
    [JSInvokable]
    public void OnFullScreenChange(bool isFullScreen)
    {
        _fullScreenCallback?.Invoke(isFullScreen);
    }

    public async ValueTask DisposeAsync()
    {
        _objRef?.Dispose();

        if (_jsModule != null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch
            {
                // 忽略 JS 互操作错误
            }
        }
    }
}