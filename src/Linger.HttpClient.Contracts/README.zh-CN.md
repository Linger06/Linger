# Linger.HttpClient.Contracts

## 目录
- [概述](#概述)
- [特性](#特性)
- [安装](#安装)
- [ApiResult与Linger.Results的关联](#apiresult与lingerresults的关联)
- [快速开始](#快速开始)
- [最佳实践](#最佳实践)
- [相关文档](#相关文档)

## 概述

**Linger.HttpClient.Contracts** 定义了HTTP客户端操作的标准接口和契约，实现**依赖倒置**和**实现灵活性**。

### 🎯 核心价值

- **解耦** - 业务逻辑与具体HTTP实现分离
- **切换** - 无缝切换不同HTTP客户端实现  
- **测试** - 轻松使用模拟实现进行单元测试
- **扩展** - 支持自定义HTTP客户端实现

### 🏗️ 架构层次

```
应用层 → IHttpClient接口 → 具体实现
      (Contracts)     (Standard/Custom)
```

## 特性

- **强类型契约**: 泛型 `ApiResult<T>` 提供类型安全的响应
- **异步支持**: 完整的async/await模式，支持 `CancellationToken`
- **错误处理**: 结构化的 `ApiResult` 错误处理框架
- **依赖注入**: 专为DI容器和HttpClientFactory设计
- **扩展性**: 易于实现自定义HTTP客户端

## 安装

```bash
# 核心契约
dotnet add package Linger.HttpClient.Contracts

# 生产实现  
dotnet add package Linger.HttpClient.Standard
```

## ApiResult与Linger.Results的关联

`ApiResult` 设计为与 `Linger.Results` 无缝集成，但**也完全兼容其他API设计**：

**与Linger.Results集成时**：
- **Error结构兼容** - `ApiResult.Errors` 与 `Result<T>.Errors` 结构一致
- **状态码映射** - HTTP状态自动对应Result错误类型
- **消息传递** - 服务端错误信息完整传递到客户端

**与其他API集成时**：
- **标准HTTP响应** - 自动解析HTTP状态码和响应体
- **灵活错误处理** - 支持任意JSON错误格式
- **通用适配** - 适用于REST、GraphQL等各种API风格

> 💡 **详细集成示例**: 参见 [StandardHttpClient 文档](../Linger.HttpClient.Standard/README.zh-CN.md#lingerresults集成)

## 快速开始

### 🚀 基本使用

```csharp
// 1. 注册实现
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 2. 注入使用
public class UserService
{
    private readonly IHttpClient _httpClient;
    
    public UserService(IHttpClient httpClient) => _httpClient = httpClient;
    
    public async Task<User?> GetUserAsync(int id)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        return result.IsSuccess ? result.Data : null;
    }
}
```

### 🔄 实现切换

```csharp
// 开发: 标准实现
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 测试: 模拟实现
services.AddSingleton<IHttpClient, MockHttpClient>();

// 生产: 弹性实现
services.AddHttpClient<IHttpClient, ResilientHttpClient>();
```

## 最佳实践

### 🏛️ 架构原则
1. **始终针对接口编程** - 使用 `IHttpClient`，从不使用具体实现
2. **在 DI 中注册实现** - 让容器管理生命周期和依赖关系  
3. **保持业务逻辑与实现无关** - 你的服务应该适用于任何 IHttpClient 实现

### 🧪 测试策略  
4. **接口模拟** - 单元测试使用Mock实现
5. **集成测试** - 使用真实实现验证HTTP行为
6. **错误测试** - 确保优雅处理网络异常

### 📊 性能考虑
7. **资源管理** - 正确实现IDisposable
8. **异步模式** - 使用ConfigureAwait(false)
9. **取消支持** - 尊重CancellationToken

---

## 📖 相关文档

- **[StandardHttpClient](../Linger.HttpClient.Standard/README.zh-CN.md)** - 生产级实现，包含详细使用示例