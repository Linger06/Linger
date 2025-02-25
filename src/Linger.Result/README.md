# Linger.Result

Linger.Result是一个轻量级库，为.NET应用程序提供了一种优雅的方式来处理操作结果，避免异常传播和空引用问题。

## 特性

- 明确区分成功和失败结果
- 支持带有强类型返回值的结果
- 友好的错误处理机制
- 支持多种结果状态：成功、错误、未找到等
- 链式操作支持

## 安装

```
dotnet add package Linger.Result
```

## 基本用法

### 创建成功结果

```csharp
// 不带值的成功结果
Result successResult = Result.Success();

// 带值的成功结果
Result<int> successWithValue = Result.Success(42);
```

### 创建失败结果

```csharp
// 不带值的失败结果
Result failureResult = Result.Failure();
Result failureWithMessage = Result.Failure("操作失败");
Result failureWithError = Result.Failure(new Error("ERR001", "发生了错误"));

// 带值类型的失败结果
Result<int> typedFailure = Result<int>.Failure("数值计算失败");
```

### 创建"未找到"结果

```csharp
Result notFoundResult = Result.NotFound();
Result<User> userNotFound = Result<User>.NotFound("未找到指定用户");
```

### 条件性创建结果

```csharp
// 基于条件创建结果
Result conditionResult = Result.Create(age >= 18);

// 基于非空检查创建结果
Result<string> nullCheckResult = Result.Create(possiblyNullString);
```

### 处理结果

```csharp
// 检查结果状态
if (result.IsSuccess)
{
    // 处理成功情况
}

// 对于带值结果，访问值
if (userResult.IsSuccess)
{
    User user = userResult.Value;
    // 使用user对象
}

// 使用模式匹配处理结果
int displayValue = userResult.Match(
    onSuccess: user => user.Score,
    onFailure: errors => -1
);

// 使用TryGetValue模式
if (userResult.TryGetValue(out User? user))
{
    // 使用user对象
}

// 使用ValueOrDefault获取值或默认值
User? userOrNull = userResult.ValueOrDefault;

// 指定自定义默认值
User defaultUser = new User("guest");
User userOrDefault = userResult.GetValueOrDefault(defaultUser);
```

## 进阶用法

### 错误处理

```csharp
// 访问错误列表
if (result.IsFailure)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"错误码: {error.Code}, 消息: {error.Message}");
    }
}
```

## 常见错误代码

- `Error.None` - 表示没有错误
- `Error.Default` - 默认错误，不具体指定
- `Error.NullValue` - 指定的结果值为空
- `Error.ConditionNotMet` - 指定的条件未满足
- `Error.NotFound` - 服务无法找到请求的资源

## 贡献

欢迎提交问题报告和Pull Request!