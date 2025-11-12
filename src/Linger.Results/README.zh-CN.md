# Linger.Results

Linger.Results 是一个现代化的操作结果处理库，采用函数式风格设计，帮助开发者更优雅地处理各种操作结果。通过使用 Result 模式而非异常机制，可以实现更可控、可预测的错误处理流程。

## 特点

- 提供清晰的成功/失败结果表示
- 支持泛型结果，可携带任意类型的返回值
- 提供丰富的函数式操作（映射、绑定、组合等）
- 强类型的错误处理，比异常更可控
- 支持 .NET 10.0、.NET 9.0、.NET 8.0、.NET Standard 2.0 和 .NET Framework 4.7.2 等多种框架

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
    
    return Result<User>.Success(user);
}

// 优雅的语法 - 利用隐式转换
public Result<User> GetUserWithValidation(string email)
{
    // 验证邮箱
    if (string.IsNullOrEmpty(email))
    {
        // 直接返回 Result.Failure，会自动转换为 Result<User>
        return Result.Failure(new Error("ValidationError", "邮箱不能为空"));
    }

    var user = _repository.FindByEmail(email);
    if (user == null)
    {
        // 同样，这里也会自动转换
        return Result.NotFound("用户不存在");
    }

    return Result<User>.Success(user);
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

### 支持 CancellationToken 的异步操作

```csharp
// 所有异步扩展方法都支持 CancellationToken 以实现可取消操作
public async Task<Result<OrderSummary>> ProcessOrderAsync(int orderId, CancellationToken cancellationToken)
{
    return await GetOrderAsync(orderId)
        // MapAsync 支持 CancellationToken
        .MapAsync(async (order, token) => 
        {
            // 执行支持取消的异步转换
            return await CalculateTotalAsync(order, token);
        }, cancellationToken)
        
        // BindAsync 支持 CancellationToken
        .BindAsync(async (total, token) => 
        {
            // 链接另一个返回 Result 的异步操作
            return await ValidatePaymentAsync(total, token);
        }, cancellationToken)
        
        // EnsureAsync 支持 CancellationToken
        .EnsureAsync(
            async (payment, token) => await CheckInventoryAsync(payment, token),
            new Error("Inventory", "库存不足"),
            cancellationToken)
        
        // MatchAsync 支持 CancellationToken
        .MatchAsync(
            async (payment, token) => 
            {
                await SendConfirmationEmailAsync(payment, token);
                return Result<OrderSummary>.Success(new OrderSummary(payment));
            },
            async (errors, token) => 
            {
                await LogErrorsAsync(errors, token);
                return Result<OrderSummary>.Failure(errors);
            },
            cancellationToken);
}

// 示例：与 HttpClient 或数据库操作一起使用
public async Task<Result<User>> UpdateUserWithCancellationAsync(
    User user, 
    CancellationToken cancellationToken)
{
    return await ValidateUser(user)
        .MapAsync(async (validUser, token) => 
        {
            // 支持取消的数据库操作
            await _dbContext.Users.AddAsync(validUser, token);
            await _dbContext.SaveChangesAsync(token);
            return validUser;
        }, cancellationToken)
        .EnsureAsync(
            async (savedUser, token) => 
            {
                // 验证保存操作
                var exists = await _dbContext.Users
                    .AnyAsync(u => u.Id == savedUser.Id, token);
                return exists;
            },
            new Error("Database", "用户保存验证失败"),
            cancellationToken);
}
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

## 隐式转换和优雅语法

Linger.Results 提供了强大的隐式转换功能，让您的代码更加简洁优雅。支持以下三种隐式转换：

### 1. 从 Result 到 Result&lt;T&gt; 的隐式转换

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    // 验证用户名
    if (string.IsNullOrEmpty(request.Username))
    {
        // 直接返回 Result.Failure，自动转换为 Result<User>
        return Result.Failure("用户名不能为空");
    }
    
    // 检查邮箱格式
    if (!IsValidEmail(request.Email))
    {
        // 使用自定义错误对象，同样会自动转换
        return Result.Failure(new Error("Email.Invalid", "邮箱格式不正确"));
    }
    
    // 检查用户是否已存在
    if (UserExists(request.Username))
    {
        // NotFound 同样支持隐式转换
        return Result.NotFound("用户名已被占用");
    }
    
    // 创建成功的情况
    var user = new User { Username = request.Username, Email = request.Email };
    return Result<User>.Success(user);
}
```

### 2. 从 Result&lt;T&gt; 到 Result 的隐式转换

```csharp
public Result ProcessUser(int userId)
{
    // 获取用户（返回 Result<User>）
    Result<User> userResult = GetUser(userId);
    
    // 自动转换为 Result，丢失具体的值但保留状态和错误信息
    Result processResult = userResult; 
    
    if (processResult.IsSuccess)
    {
        // 执行处理逻辑
        return Result.Success();
    }
    
    // 错误信息被保留
    return processResult;
}
```

### 3. 从 T 到 Result&lt;T&gt; 的隐式转换

```csharp
public Result<User> GetDefaultUser()
{
    var defaultUser = new User { Name = "默认用户", Email = "default@example.com" };
    
    // 对象自动转换为成功的 Result<User>
    return defaultUser;
}

public Result<string> GetConfigValue(string key)
{
    string value = _configuration[key];
    
    // 如果值为 null，自动创建失败结果
    // 如果值不为 null，自动创建成功结果
    return value; // 等价于 Result<string>.Create(value)
}
```

### 链式转换示例

```csharp
// 演示不同的返回方式都能隐式转换为 Result<User>
private Result<User> GetUserById(int id)
{
    return id switch
    {
        1 => _testUser,                     // User → Result<User>
        0 => Result.Success(),              // Result → Result<User>  
        _ => Result.Failure("User not found") // Result → Result<User>
    };
}

// 可以自由在不同结果类型间转换
private Result ProcessUserData(Result<User> userResult)
{
    // Result<User> → Result
    return userResult; 
}
```

### 隐式转换的重要说明

⚠️ **重要提示**：
- `Result<T>` → `Result` 转换会**丢失值信息**，因为非泛型Result不保存具体值
- `T` → `Result<T>` 转换中，如果值为 `null` 会自动创建失败结果
- 失败的 `Result<T>` 访问 `.Value` 属性会抛出 `InvalidOperationException`
- 建议使用 `.ValueOrDefault` 或 `.TryGetValue()` 进行安全的值访问

```csharp
// 正确的用法示例
Result<User> userResult = GetUser(123);

// ✅ 安全访问值
if (userResult.TryGetValue(out var user))
{
    Console.WriteLine($"用户：{user.Name}");
}

// ✅ 使用默认值
var safeUser = userResult.ValueOrDefault;

// ❌ 危险：如果结果失败会抛出异常
var user = userResult.Value; // 可能抛出 InvalidOperationException
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
        return Result.Failure(UserErrors.NotFound); // 隐式转换为 Result<User>
        
    if (!ValidatePassword(password, user.PasswordHash))
        return Result.Failure(UserErrors.InvalidCredentials); // 隐式转换为 Result<User>
        
    return Result<User>.Success(user);
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

1. **优先使用 `Result` 和 `Result<T>` 类，而非异常**：
   - 对于预期内的错误（如验证错误、未找到资源等）返回 Result
   - 只对真正的异常情况（程序错误、未预期的系统故障）使用异常机制

2. **返回值保持一致性**：
   - 服务方法应始终返回 `Result` 或 `Result<T>`，而非混合使用结果和异常
   - 保持统一的错误处理模式，便于调用方处理

3. **使用有意义的错误代码**：
   - 定义领域特定的错误常量
   - 使用结构化的错误代码（如 "Category.SubCategory.Error"）

4. **利用隐式转换简化代码**：
   - 在返回 `Result<T>` 的方法中，可以直接返回 `Result.Failure()` 或 `Result.NotFound()`
   - 可以直接返回对象实例，会自动转换为成功的 `Result<T>`
   - 注意 `null` 值会自动转换为失败结果
   - 这样可以让代码更简洁优雅，同时保持类型安全

5. **正确使用 Value 属性**：
   - 始终在访问 `.Value` 前检查 `.IsSuccess`
   - 优先使用 `.ValueOrDefault` 或 `.TryGetValue()` 进行安全访问
   - 避免在失败的结果上访问 `.Value`，这会抛出 `InvalidOperationException`

6. **利用链式操作**：
   - 使用函数式方法组合而非传统的条件语句
   - Map、Bind、Tap等方法可以极大提高代码可读性

7. **对于Web API**：
   - 结合 [Linger.Results.AspNetCore](../Linger.Results.AspNetCore/README.zh-CN.md) 包转换为HTTP响应
   - 使用ProblemDetails格式返回标准化的错误响应

8. **隐式转换最佳实践**：
   - 了解转换过程中的信息丢失（如 `Result<T>` → `Result` 会丢失值）
   - 合理利用对象到结果的自动转换简化代码
   - 注意 `null` 检查，避免意外的失败结果

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
