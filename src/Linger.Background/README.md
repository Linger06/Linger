# Linger.Background

一个轻量级的.NET后台任务处理库。

## 介绍

Linger.Background为.NET开发者提供了简单的工具和扩展，用于在.NET应用程序中管理和执行基本的后台任务。

## 功能

- 基础后台任务队列
- 简单的工作者处理机制
- 任务取消支持
- 与.NET依赖注入集成

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

## 安装

### Package Manager命令行

```
PM> Install-Package Linger.Background
```

### .NET CLI命令行

```
> dotnet add package Linger.Background
```
