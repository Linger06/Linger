namespace Linger.Blazor.Services;

/// <summary>
/// 全屏操作服务接口，用于抽象不同平台的全屏实现
/// </summary>
public interface IFullScreenService
{
    /// <summary>
    /// 切换全屏状态
    /// </summary>
    Task ToggleFullScreenAsync();

    /// <summary>
    /// 获取当前是否处于全屏状态
    /// </summary>
    Task<bool> IsFullScreenAsync();

    /// <summary>
    /// 初始化全屏状态监听
    /// </summary>
    /// <param name="callback">全屏状态变化回调</param>
    Task InitializeFullScreenListenerAsync(Action<bool> callback);
}