````markdown
# Linger.Background

一个轻量级的.NET后台任务处理库。

## 介绍

Linger.Background为.NET开发者提供了简单的工具和扩展，用于在.NET应用程序中管理和执行基本的后台任务。

## 功能

- 基础后台任务队列
- 简单的工作者处理机制
- 任务取消支持
- 与.NET依赖注入集成
- 支持从 IServiceProvider 获取服务

## 使用示例

```csharp
// 注册服务
public void ConfigureServices(IServiceCollection services)
{
    // 根据实际项目中的扩展方法来注册Background服务
    services.AddHostedService<QueuedHostedService>();
    services.AddSingleton<IBackgroundTaskQueue>(ctx =>
    {
        //if (!int.TryParse(hostContext.Configuration["QueueCapacity"], out var queueCapacity))
        var queueCapacity = 100;
        return new BackgroundTaskQueue(queueCapacity);
    });
}

// 使用后台任务队列
public class SampleService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    
    public SampleService(IBackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }
    
    public async Task EnqueueWorkItem()
    {
        // 注意: Lambda 接收两个参数 (IServiceProvider, CancellationToken)
        await _taskQueue.QueueBackgroundWorkItemAsync(async (serviceProvider, token) =>
        {
            // 可以从 serviceProvider 获取服务
            var logger = serviceProvider.GetRequiredService<ILogger<SampleService>>();
            
            // 执行后台工作，支持取消
            await Task.Delay(1000, token);
            logger.LogInformation("后台任务执行完成");
        });
    }
    
    // 支持外部取消令牌
    public async Task EnqueueWorkItemWithCancellation(CancellationToken cancellationToken)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(
            async (serviceProvider, token) =>
            {
                // 执行后台工作
                await Task.Delay(1000, token);
            },
            cancellationToken); // 可选的取消令牌参数
    }
}
```

### 高级用法

```csharp
public class BackgroundJobService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    
    public BackgroundJobService(IBackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }
    
    // 处理复杂的后台任务
    public async Task ProcessLargeDataSet(List<int> dataIds, CancellationToken cancellationToken = default)
    {
        foreach (var dataId in dataIds)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(
                async (serviceProvider, token) =>
                {
                    // 从服务提供者获取所需服务
                    var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                    var logger = serviceProvider.GetRequiredService<ILogger<BackgroundJobService>>();
                    
                    try
                    {
                        // 处理数据
                        var data = await dbContext.Data.FindAsync(dataId, token);
                        if (data != null)
                        {
                            // 执行业务逻辑
                            await ProcessDataAsync(data, token);
                            logger.LogInformation("数据 {DataId} 处理成功", dataId);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogWarning("数据 {DataId} 处理被取消", dataId);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "处理数据 {DataId} 时发生错误", dataId);
                    }
                },
                cancellationToken);
        }
    }
}
```

## API 说明

### IBackgroundTaskQueue 接口

```csharp
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 将后台工作项加入队列
    /// </summary>
    /// <param name="workItem">
    /// 工作项函数，接收 IServiceProvider 和 CancellationToken 参数
    /// </param>
    /// <param name="cancellationToken">可选的取消令牌</param>
    ValueTask QueueBackgroundWorkItemAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从队列中取出工作项
    /// </summary>
    ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}
```

### 重要说明

1. **工作项签名**: 工作项必须是 `Func<IServiceProvider, CancellationToken, ValueTask>` 类型
   - 第一个参数: `IServiceProvider` - 用于获取依赖注入的服务
   - 第二个参数: `CancellationToken` - 用于取消操作

2. **取消支持**: 
   - `QueueBackgroundWorkItemAsync` 方法接受可选的 `CancellationToken` 参数
   - 取消令牌用于在加入队列时检查是否已请求取消
   - 工作项内部应正确处理 `CancellationToken` 以支持优雅取消

3. **服务解析**:
   - 通过 `IServiceProvider` 参数可以获取注册的服务
   - 使用作用域服务时要注意服务的生命周期

## 安装

### Package Manager命令行

```
PM> Install-Package Linger.Background
```

### .NET CLI命令行

```
> dotnet add package Linger.Background
```

## 依赖项

- .NET 8.0+
- .NET 9.0+
- .NET 10.0+

## 许可证

本项目根据 Linger 项目提供的许可条款授权。
````