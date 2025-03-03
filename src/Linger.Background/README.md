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
    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
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
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            // 执行后台工作
            await Task.Delay(1000, token);
        });
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
