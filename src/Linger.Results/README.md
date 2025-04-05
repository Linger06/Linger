# Linger.Results

Linger.Results 是一个现代化的操作结果处理库，采用函数式风格设计，帮助开发者更优雅地处理各种操作结果。通过使用 Result 模式而非异常机制，可以实现更可控、可预测的错误处理流程。

## 特点

- 提供清晰的成功/失败结果表示
- 支持泛型结果，可携带任意类型的返回值
- 提供丰富的函数式操作（映射、绑定、组合等）
- 强类型的错误处理，比异常更可控
- 支持 .NET 9.0、.NET 8.0、.NET Standard 2.0 和 .NET Framework 4.7.2 等多种框架

## 安装

```shell
dotnet add package Linger.Results
```

## 基本用法

### 创建结果

```csharp
// 创建成功结果
var success = Result.Success();
var successWithValue = Result.Success(42);

// 创建失败结果
var failure = Result.Failure("操作失败");
var failureWithError = Result.Failure(new Error("ErrorCode", "详细错误信息"));

// 创建未找到结果
var notFound = Result.NotFound();
```

### 使用泛型结果

```csharp
// 定义返回用户信息的方法
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result<User>.NotFound($"找不到ID为{id}的用户");
    
    return Result.Success(user);
}

// 使用方法
var result = GetUser(123);
if (result.IsSuccess)
{
    var user = result.Value; // 只有在结果成功时才能访问值
    Console.WriteLine($"找到用户: {user.Name}");
}
else
{
    Console.WriteLine($"错误: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

### 使用 Match 方法

```csharp
// 使用 Match 方法处理不同的结果状态
string displayName = result.Match(
    user => $"用户: {user.Name}",
    errors => $"错误: {string.Join(", ", errors.Select(e => e.Message))}"
);
```

### 安全访问值

```csharp
// 使用TryGetValue安全地访问结果值
if (result.TryGetValue(out var user))
{
    // 成功获取用户
    Console.WriteLine($"用户: {user.Name}");
}

// 使用ValueOrDefault获取值或默认值
var safeUser = result.ValueOrDefault; // 失败时为null

// 指定默认值
var userOrGuest = result.GetValueOrDefault(new User { Name = "访客" });
```

### 链式操作

```csharp
// 使用扩展方法进行链式操作
var finalResult = GetUser(123)
    .Map(user => user.Email)
    .Bind(email => SendEmail(email))
    .Ensure(success => success, new Error("EmailError", "邮件发送失败"));
```

### 异步支持

```csharp
// 异步操作
var result = await GetUserAsync(123)
    .MapAsync(async user => await GetUserPreferencesAsync(user))
    .BindAsync(async prefs => await UpdatePreferencesAsync(prefs));
```

### 使用 Result.Create 进行条件判断

```csharp
// 基于布尔条件创建结果
public Result ValidatePassword(string password)
{
    return Result.Create(password.Length >= 8)
        .Ensure(() => password.Any(char.IsUpper), new Error("Password", "密码必须包含大写字母"))
        .Ensure(() => password.Any(char.IsDigit), new Error("Password", "密码必须包含数字"));
}
```

### 错误处理

```csharp
// 使用 Try 方法捕获异常并转换为结果
var result = ResultFunctionalExtensions.Try(
    () => SomeOperationThatMightThrow(),
    ex => ex.ToError()
);
```

## 高级用法

### 组合多个结果

```csharp
// 组合多个结果，所有结果成功时才成功
var combinedResult = Result.Combine(
    ValidateUsername(request.Username),
    ValidateEmail(request.Email),
    ValidatePassword(request.Password)
);

if (combinedResult.IsSuccess)
{
    // 所有验证都通过
    return Result.Success(new User { /* ... */ });
}
```

### 自定义错误类型

```csharp
// 定义特定领域的错误类型
public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "用户不存在");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "用户名或密码不正确");
    public static readonly Error DuplicateEmail = new("User.DuplicateEmail", "邮箱已被使用");
}

// 使用自定义错误
public Result<User> Authenticate(string username, string password)
{
    var user = _repository.FindByUsername(username);
    if (user == null)
        return Result<User>.Failure(UserErrors.NotFound);
        
    if (!ValidatePassword(password, user.PasswordHash))
        return Result<User>.Failure(UserErrors.InvalidCredentials);
        
    return Result.Success(user);
}
```

### 条件分支处理

```csharp
public async Task<Result<OrderConfirmation>> ProcessOrder(Order order)
{
    // 链式处理订单流程
    return await ValidateOrder(order)
        .BindAsync(async validOrder => 
        {
            // 根据支付方式选择不同处理路径
            if (validOrder.PaymentMethod == PaymentMethod.CreditCard)
                return await ProcessCreditCardPayment(validOrder);
            else if (validOrder.PaymentMethod == PaymentMethod.BankTransfer)
                return await ProcessBankTransfer(validOrder);
            else
                return Result<OrderConfirmation>.Failure("不支持的支付方式");
        })
        .TapAsync(async confirmation => 
        {
            // 成功时执行副作用操作，但不改变结果
            await SendConfirmationEmail(confirmation);
            await UpdateInventory(order);
        });
}
```

## ResultStatus 枚举扩展

默认的 `ResultStatus` 枚举包含 `Ok`、`NotFound` 和 `Error`。您可以根据需要扩展此枚举以包含更多状态：

```csharp
// 在您的项目中创建一个部分类扩展
namespace Linger.Results
{
    public enum ResultStatus
    {
        // 已有的状态
        Ok,
        NotFound,
        Error,
        
        // 新增自定义状态
        Unauthorized,
        Forbidden,
        Conflict,
        ValidationError
    }
}
```

## 最佳实践

1. **优先使用 Result 和 Result<T> 类，而非异常**：
   - 对于预期内的错误（如验证错误、未找到资源等）返回 Result
   - 只对真正的异常情况（程序错误、未预期的系统故障）使用异常机制

2. **返回值保持一致性**：
   - 服务方法应始终返回 Result 或 Result<T>，而非混合使用结果和异常
   - 保持统一的错误处理模式，便于调用方处理

3. **使用有意义的错误代码**：
   - 定义领域特定的错误常量
   - 使用结构化的错误代码（如 "Category.SubCategory.Error"）

4. **利用链式操作**：
   - 使用函数式方法组合而非传统的条件语句
   - Map、Bind、Tap等方法可以极大提高代码可读性

5. **对于Web API**：
   - 结合 [Linger.Results.AspNetCore](../Linger.Results.AspNetCore/README.md) 包转换为HTTP响应
   - 使用ProblemDetails格式返回标准化的错误响应

## 和异常处理对比

| 方面 | Result 模式 | 异常机制 |
|------|------------|---------|
| 可见性 | 明确的返回类型，编译时可见 | 隐式抛出，运行时才知道 |
| 性能 | 较好，无堆栈捕获开销 | 较差，尤其在高频调用场景 |
| 可组合性 | 优秀，支持链式和组合操作 | 弱，需要多层try-catch |
| 类型安全 | 强类型，编译器辅助 | 弱类型，基于字符串匹配 |
| 适用场景 | 业务逻辑，预期内的错误 | 程序错误，未预期的异常 |

## 许可证

MIT