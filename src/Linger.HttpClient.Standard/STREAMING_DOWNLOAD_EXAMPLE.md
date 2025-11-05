# HttpClient 流式下载优化示例

## 问题描述

在之前的实现中,使用 `CallApi<byte[]>` 下载大文件时,会将整个文件内容加载到内存中,导致内存占用过高。例如下载 500MB 文件时,内存峰值可达 500MB+。

## 解决方案

通过引入 `HttpResponseMode` 枚举和流式下载方法,支持以下两种模式:

1. **Buffered Mode** (默认): 使用 `HttpCompletionOption.ResponseContentRead`,完整读取响应内容后返回
2. **Streamed Mode**: 使用 `HttpCompletionOption.ResponseHeadersRead`,仅读取响应头后立即返回 Stream

## 使用示例

### 1. 流式下载到内存流 (DownloadStreamAsync)

```csharp
using var client = new StandardHttpClient("https://example.com", logger);

// 下载大文件,返回 Stream(调用方负责释放)
var result = await client.DownloadStreamAsync("/large-file.zip");

if (result.IsSuccess && result.Data is not null)
{
    using var stream = result.Data;
    
    // 方式 1: 直接处理流
    var buffer = new byte[8192];
    int bytesRead;
    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
    {
        // 处理数据块
        ProcessChunk(buffer, bytesRead);
    }
    
    // 方式 2: 复制到文件
    using var fileStream = File.Create("output.zip");
    await stream.CopyToAsync(fileStream);
}
```

### 2. 流式下载到文件 (DownloadToFileAsync) - 推荐

```csharp
using var client = new StandardHttpClient("https://example.com", logger);

// 创建进度报告器
var progress = new Progress<(long downloaded, long? total)>(p =>
{
    if (p.total.HasValue)
    {
        var percent = (double)p.downloaded / p.total.Value * 100;
        Console.WriteLine($"已下载: {p.downloaded:N0} / {p.total:N0} 字节 ({percent:F1}%)");
    }
    else
    {
        Console.WriteLine($"已下载: {p.downloaded:N0} 字节");
    }
});

// 下载到文件,内存占用仅为缓冲区大小 (默认 8KB)
var result = await client.DownloadToFileAsync(
    url: "/large-file.zip",
    destinationPath: "output.zip",
    timeout: 300, // 5 分钟超时
    bufferSize: 81920, // 可选: 80KB 缓冲区(默认 8KB)
    progress: progress,
    cancellationToken: cancellationToken
);

if (result.IsSuccess)
{
    Console.WriteLine("下载完成!");
}
else
{
    Console.WriteLine($"下载失败: {result.ErrorMsg}");
}
```

### 3. 直接使用 CallApiWithMode (高级用法)

```csharp
using var client = new StandardHttpClient("https://example.com", logger);

// 流式模式 - 内存占用最小
var streamResult = await client.CallApiWithMode<Stream>(
    url: "/large-file.zip",
    method: HttpMethodEnum.Get,
    responseMode: HttpResponseMode.Streamed, // 关键参数
    cancellationToken: cancellationToken
);

if (streamResult.IsSuccess && streamResult.Data is not null)
{
    using var stream = streamResult.Data;
    // 处理流...
}

// 缓冲模式 - 完整读取内容(适合小文件)
var bufferedResult = await client.CallApiWithMode<byte[]>(
    url: "/small-file.json",
    method: HttpMethodEnum.Get,
    responseMode: HttpResponseMode.Buffered // 默认值
);
```

## 性能对比

### 下载 500MB 文件

| 方法 | 内存占用 | 速度 | 备注 |
|------|---------|------|------|
| `CallApi<byte[]>` (旧) | ~500MB | 基准 | 将整个文件加载到内存 |
| `DownloadStreamAsync` (新) | ~8KB | 类似 | 仅缓冲区内存占用 |
| `DownloadToFileAsync` (新) | ~8-80KB | 类似 | 可自定义缓冲区大小 |

### 内存优化效果

- **内存减少**: 99.99% (500MB → 8KB)
- **支持超大文件**: 理论上无限制(仅受磁盘空间限制)
- **响应速度**: 更快(无需等待完整下载)

## 注意事项

1. **Stream 生命周期管理**
   - 使用 `DownloadStreamAsync` 时,调用方必须手动释放返回的 Stream
   - 推荐使用 `using` 语句确保正确释放资源
   - `DownloadToFileAsync` 会自动管理 Stream 生命周期

2. **取消令牌支持**
   ```csharp
   using var cts = new CancellationTokenSource();
   cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30秒后自动取消
   
   var result = await client.DownloadToFileAsync(
       url: "/large-file.zip",
       destinationPath: "output.zip",
       cancellationToken: cts.Token
   );
   ```

3. **向后兼容性**
   - 现有的 `CallApi<T>` 方法保持不变,默认使用 Buffered 模式
   - 不会影响现有代码的行为

4. **错误处理**
   ```csharp
   var result = await client.DownloadToFileAsync(url, path);
   
   if (!result.IsSuccess)
   {
       Console.WriteLine($"错误: {result.ErrorMsg}");
       Console.WriteLine($"状态码: {result.StatusCode}");
       
       foreach (var error in result.Errors)
       {
           Console.WriteLine($"  - {error.Code}: {error.Message}");
       }
   }
   ```

## 实现原理

### HttpCompletionOption 的作用

```csharp
// Buffered 模式 (默认)
var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
// ✅ 等待响应体完全下载
// ❌ 内存占用 = 响应体大小

// Streamed 模式 (优化)
var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
// ✅ 仅读取响应头就返回
// ✅ 内存占用 = 缓冲区大小
// ✅ 可以立即开始处理流
```

### DeserializeResponseContent 类型处理

```csharp
protected virtual async Task<T> DeserializeResponseContent<T>(HttpResponseMessage response)
{
    var targetType = typeof(T);

    // 流式处理 - 直接返回 Stream,不读取内容
    if (targetType == typeof(Stream))
    {
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return (T)(object)stream;
    }

    // 字节数组 - 完整读取内容
    if (targetType == typeof(byte[]))
    {
        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return (T)(object)bytes;
    }

    // 其他类型...
}
```

### HandleResponseMessage 资源管理

```csharp
protected virtual async Task<ApiResult<T>> HandleResponseMessage<T>(HttpResponseMessage res)
{
    try
    {
        // 处理响应...
    }
    finally
    {
        // 当 T 是 Stream 时不释放 HttpResponseMessage
        // 因为 Stream 依赖于 HttpResponseMessage 的生命周期
        if (typeof(T) != typeof(HttpResponseMessage) && typeof(T) != typeof(Stream))
        {
            res.Dispose();
        }
    }
}
```

## 相关改动

### 新增文件
- `src/Linger.HttpClient.Contracts/Core/HttpResponseMode.cs` - 响应模式枚举

### 修改文件
- `src/Linger.HttpClient.Contracts/Core/HttpClientBase.cs`
  - 新增 `CallApiWithMode<T>` 抽象方法
  - 新增 `DownloadStreamAsync` 便捷方法
  - 新增 `DownloadToFileAsync` 便捷方法(支持进度报告)
  - 修改 `DeserializeResponseContent<T>` 支持 Stream 类型
  - 修改 `HandleResponseMessage<T>` 支持 Stream 资源管理

- `src/Linger.HttpClient.Standard/StandardHttpClient.cs`
  - `CallApi<T>` 内部调用 `CallApiWithMode<T>` (Buffered 模式)
  - 实现 `CallApiWithMode<T>` 方法,根据 responseMode 选择 HttpCompletionOption

## 总结

这次优化完美解决了大文件下载的内存占用问题:

✅ **内存优化**: 从 100% 文件大小降低到 ~8KB 缓冲区  
✅ **性能提升**: 更快的响应速度(无需等待完整下载)  
✅ **功能增强**: 支持进度报告、取消令牌  
✅ **向后兼容**: 现有代码无需修改  
✅ **代码质量**: 遵循 C# 最佳实践和项目规范  
