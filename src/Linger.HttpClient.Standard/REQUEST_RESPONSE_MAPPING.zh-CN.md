# 调用流程与返回映射

本章详细说明：WebAPI 如何返回响应、HttpClient.CallApi 如何调用，以及 ApiResult<T> 的各字段如何填充。

## 约定总则（建议）

- 服务端统一返回 `Result<T>` / `Result`，控制器用 `ToActionResult()` / `ToHttpResult()` 做响应映射。
- 错误优先采用 `ProblemDetails`（含 `errors`），便于客户端映射到 `ApiResult.Errors`。
- 成功响应尽量保持结构稳定：有数据用 `ActionResult<T>`，无数据用 `ActionResult` + 204。
- 客户端调用：有数据用 `GetAsync<T>` / `CallApi<T>`；无数据使用非泛型 `PostAsync` / `PutAsync` / `DeleteAsync`，只关心 `IsSuccess` / `StatusCode`。

补充要点：
- `IsSuccess` 要求 HTTP 状态为 2xx，且不存在传输或解析错误。
- 成功但响应体为空时，`CallApi<T>` 返回默认值；`CallApi<object>` 返回 `null`。

## 错误字段格式（重要）

服务端应尽量采用 RFC 7807 的 `ProblemDetails` 或框架默认的字段验证格式，且 `errors` 的值为字符串数组（`string[]`），以兼容客户端对同一字段的多条提示。

说明：服务端在实现时推荐使用 `Dictionary<string, string[]>`（或框架默认的 ModelState 验证结构），以保证与 `ProblemDetailsWithErrors`/`CreateProblemDetails` 的序列化兼容。

示例（ProblemDetails，含字段错误数组）：

```json
HTTP/1.1 422 Unprocessable Entity
Content-Type: application/problem+json
{
    "title": "One or more validation errors occurred.",
    "status": 422,
    "detail": "The request contains invalid data.",
    "errors": {
        "Email": ["Invalid email format"],
        "Age": ["Age must be greater than 18"]
    }
}
```

客户端映射要点：

- `HttpClient` 将 `errors` 展开为 `ApiResult.Errors`（一个 `Error` 列表），每个 `Error` 的 `Code` 对应字段名，`Message` 对应数组中的单条信息。
- `ApiResult.ErrorMsg` 的生成规则：优先使用 `ProblemDetails.Detail`；若无则使用 `errors` 中的首条消息。客户端不会自动把多条错误合并为一个长文本（需要时可自行合并显示）。

注意：客户端为防御性读取响应体，解析错误体时最多读取前 25K 个字符，因此在返回非常大的非结构化响应时，客户端只会获取响应前部用于展示或作为 `ErrorMsg` 的兜底文本。

示例：服务端返回上述 ProblemDetails，客户端获得的 `ApiResult`：

- `IsSuccess` = false
- `StatusCode` = 422
- `ErrorMsg` = "Invalid email format"（优先使用 `ProblemDetails.detail`；若无，则使用 `errors` 数组的首条消息）
- `Errors` = [ Error{ Code="Email", Message="Invalid email format" }, Error{ Code="Age", Message="Age must be greater than 18"} ]
失败时 `ErrorMsg` 优先使用 `ProblemDetails.Detail` 或 `Errors` 中的首条消息；解析结构化错误体失败则回退为原始响应文本或状态码默认消息。
- `ToActionResult` / `ToHttpResult` 默认失败状态码为 400 或 404（按 ResultStatus 映射）；需要 422/409 等状态码时可显式传入 `failureStatusCode`。

### 场景 1：成功情况

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：成功时返回结果值
[HttpGet("/api/users/{id}")]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    return result.ToActionResult();
}

// Minimal API：成功时 ToHttpResult 返回 200 OK
app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToHttpResult();
});
```

**WebAPI 返回 200 OK 且包含有效的 JSON 数据**

```
WebAPI 响应:
HTTP/1.1 200 OK
Content-Type: application/json
{
    "id": 123,
    "name": "张三",
    "email": "zhangsan@example.com"
}
```

**客户端调用与返回值：**

```csharp
// 客户端调用
var result = await _httpClient.GetAsync<User>("api/users/123", cancellationToken: ct);

// 返回值字段映射:
// result.IsSuccess       = true
// result.Data            = User { Id = 123, Name = "张三", Email = "zhangsan@example.com" }
// result.StatusCode      = 200
// result.ErrorMsg        = null
// result.Errors          = 空数组

if (result.IsSuccess && result.Data is not null)
{
    var user = result.Data; // 直接使用反序列化的对象
    Console.WriteLine($"用户: {user.Name}");
}
```

### 场景 1.1：成功但不返回业务数据

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：成功时返回 204 No Content
[HttpDelete("/api/users/{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    return result.ToNoContentResult();
}

// Minimal API：成功时返回 204 No Content
app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.DeleteUserAsync(id);
    return result.ToNoContentResult();
});
```

**WebAPI 返回 204 No Content，服务端使用非泛型 Result**

```
WebAPI 响应:
HTTP/1.1 204 No Content
```

**客户端调用与返回值：**

```csharp
// 客户端调用 DELETE 请求，不需要返回业务数据
var result = await _httpClient.CallApi<object>(
    "api/users/123",
    HttpMethod.Delete,
    cancellationToken: ct
);

// 返回值字段映射:
// result.StatusCode      = 204
// result.ErrorMsg        = null
// result.Errors          = 空数组
// 这里使用 CallApi<object> / ApiResult<object>，不需要读取 result.Data

if (result.StatusCode == HttpStatusCode.NoContent)
{
    Console.WriteLine("删除成功");
}
```

### 场景 2：参数或验证错误（ProblemDetails）

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：失败时 ToActionResult 自动返回 ProblemDetails
[HttpPost("/api/users")]
public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request);
    return result.ToActionResult(failureStatusCode: HttpStatusCode.UnprocessableEntity);
}

// Minimal API：失败时 ToHttpResult 自动返回 ProblemDetails
app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    return result.ToHttpResult(failureStatusCode: HttpStatusCode.UnprocessableEntity);
});
```

**WebAPI 返回 422 且包含验证错误**

```
WebAPI 响应:
HTTP/1.1 422 Unprocessable Entity
Content-Type: application/problem+json
{
    "title": "One or more validation errors occurred.",
    "status": 422,
    "errors": {
        "Email": ["邮箱格式不正确"],
        "Age": ["年龄必须大于 18"]
    }
}
```

**客户端调用与返回值：**

```csharp
// 客户端调用 POST 请求，传入无效数据
var invalidUser = new User { Email = "invalid-email", Age = 10 };
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethod.Post,
    requestBody: invalidUser
);

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null（因为 IsSuccess=false）
// result.StatusCode      = 422
// result.ErrorMsg        = "邮箱格式不正确"（优先使用 `ProblemDetails.detail`；若无，则使用 `errors` 的首条字段错误消息）
// result.Errors          = [
//     Error { Code = "Email", Message = "邮箱格式不正确" },
//     Error { Code = "Age", Message = "年龄必须大于 18" }
// ]

if (!result.IsSuccess)
{
    // 直接显示全局错误
    Console.WriteLine($"验证失败: {result.ErrorMsg}");

    // 或逐项显示用于表单内联提示
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"字段 {error.Code}: {error.Message}");
    }
}
```

### 场景 3：业务/全局错误（ProblemDetails）

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：业务规则失败时自动返回 ProblemDetails
[HttpPost("/api/orders/submit")]
public async Task<ActionResult<OrderDto>> SubmitOrder(OrderSubmitRequest request)
{
    var result = await _orderService.SubmitAsync(request);
    return result.ToActionResult(failureStatusCode: HttpStatusCode.Conflict);
}

// Minimal API：失败时 ToHttpResult 自动返回 ProblemDetails
app.MapPost("/api/orders/submit", async (OrderSubmitRequest request, IOrderService orderService) =>
{
    var result = await orderService.SubmitAsync(request);
    return result.ToHttpResult(failureStatusCode: HttpStatusCode.Conflict);
});

```

**WebAPI 返回 409 Conflict 且包含 ProblemDetails**

```
WebAPI 响应:
HTTP/1.1 409 Conflict
Content-Type: application/problem+json
{
    "title": "A conflict occurred",
    "status": 409,
    "detail": "Please refer to the errors property for additional details.",
    "errors": {
        "InsufficientStock": ["库存不足，需求 10 件但仅剩 5 件"],
        "PaymentGatewayDown": ["支付网关暂时不可用，请稍后重试"]
    }
}
```

**客户端调用与返回值：**

```csharp
// 客户端调用 POST 请求，提交订单
var order = new OrderSubmitRequest { /* ... */ };
var result = await _httpClient.CallApi<Order>(
    "api/orders/submit",
    HttpMethod.Post,
    requestBody: order
);

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null（因为 IsSuccess=false）
// result.StatusCode      = 409
// result.ErrorMsg        = "Please refer to the errors property for additional details."
// result.Errors          = [
//     Error { Code = "InsufficientStock", Message = "库存不足，需求 10 件但仅剩 5 件" },
//     Error { Code = "PaymentGatewayDown", Message = "支付网关暂时不可用，请稍后重试" }
// ]

if (!result.IsSuccess)
{
    // 全局信息使用 ProblemDetails.detail；具体业务错误项从 Errors 逐一处理
    Console.WriteLine($"订单提交失败: {result.ErrorMsg}");

    // 逐项访问具体错误编码以进行不同处理
    foreach (var error in result.Errors)
    {
        switch (error.Code)
        {
            case "InsufficientStock":
                Console.WriteLine("请重新调整购物车数量");
                break;
            case "PaymentGatewayDown":
                Console.WriteLine("请稍后重试或更换支付方式");
                break;
        }
    }
}
```

#### 旧服务端错误数组兼容

客户端暂时保留对以下旧格式的解析，但新接口不应再主动返回该格式：

```json
[
    { "code": "InsufficientStock", "message": "库存不足" },
    { "code": "PaymentGatewayDown", "message": "支付网关暂时不可用" }
]
```

旧数组同样会展开到 `ApiResult.Errors`，首条错误消息作为 `ErrorMsg`。

### 场景 4：HTTP 错误（4xx / 5xx 无结构化错误体）

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：直接返回纯文本或无结构化错误体
[HttpPost("/api/reports/generate")]
public IActionResult GenerateReport()
{
    return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error occurred");
}

// Minimal API：直接返回文本
app.MapPost("/api/reports/generate", () =>
    Results.Text("Internal server error occurred", "text/plain", StatusCodes.Status500InternalServerError));
```

**WebAPI 返回 500 且无法解析错误体（或纯文本）**

```
WebAPI 响应:
HTTP/1.1 500 Internal Server Error
Content-Type: text/plain
Internal server error occurred
```

**客户端调用与返回值：**

```csharp
// 客户端调用
var result = await _httpClient.GetAsync<ReportData>("api/reports/generate");

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null
// result.StatusCode      = 500
// result.ErrorMsg        = "Internal server error"
// result.Errors          = [Error { Code = "", Message = "Internal server error occurred" }]

if (!result.IsSuccess)
{
    if (result.StatusCode == HttpStatusCode.InternalServerError)
    {
        Console.WriteLine($"服务器错误: {result.ErrorMsg}");
        Console.WriteLine("请稍后重试或联系管理员");
    }
}
```

### 场景 5：自定义错误格式

**服务端返回示例（Controller / Minimal API）**

```csharp
// Controller：返回自定义错误格式
[HttpPost("/api/orders/confirm")]
public IActionResult ConfirmOrder(ConfirmOrderRequest request)
{
    var error = new { error_code = "CUSTOM_ERROR", error_message = "自定义错误信息", details = "这是一个自定义格式的错误" };
    return StatusCode(StatusCodes.Status400BadRequest, error);
}

// Minimal API：同样返回自定义结构
app.MapPost("/api/orders/confirm", (ConfirmOrderRequest request) =>
{
    var error = new { error_code = "CUSTOM_ERROR", error_message = "自定义错误信息", details = "这是一个自定义格式的错误" };
    return Results.Json(error, statusCode: StatusCodes.Status400BadRequest);
});
```

**WebAPI 返回自定义格式的错误（既不是 ProblemDetails 也不是旧错误数组）**

```
WebAPI 响应:
HTTP/1.1 400 Bad Request
Content-Type: application/json
{
    "error_code": "CUSTOM_ERROR",
    "error_message": "自定义错误信息",
    "details": "这是一个自定义格式的错误"
}
```

**客户端需要继承 StandardHttpClient 来处理自定义格式：**

```csharp
public class CustomHttpClient : StandardHttpClient
{
    public CustomHttpClient(HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
        : base(httpClient, logger)
    {
    }

    protected override async Task<(string ErrorMessage, IEnumerable<Error> Errors)> GetErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        try
        {
            // 尝试解析自定义格式
            var customError = JsonSerializer.Deserialize<CustomErrorFormat>(responseText);
            if (customError is not null)
            {
                var errorMsg = $"{customError.ErrorCode}: {customError.ErrorMessage}";
                var errors = new[] { new Error(customError.ErrorCode, customError.Details) };
                return (errorMsg, errors);
            }
        }
        catch (JsonException)
        {
            // 解析失败，回退到默认处理
        }

        // 回退到默认的 ProblemDetails / 旧错误数组处理
        return await base.GetErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private record CustomErrorFormat(string ErrorCode, string ErrorMessage, string Details);
}

// 使用自定义客户端
services.AddHttpClient<IHttpClient, CustomHttpClient>();

// 返回值字段映射完全相同:
// result.IsSuccess       = false
// result.StatusCode      = 400
// result.ErrorMsg        = "CUSTOM_ERROR: 自定义错误信息"
// result.Errors          = [
//     Error { Code = "CUSTOM_ERROR", Message = "这是一个自定义格式的错误" }
// ]
```

### ApiResult<T> 字段对照表

| 字段 | 类型 | 成功时 | 失败时 | 说明 |
|------|------|--------|--------|------|
| `IsSuccess` | `bool` | `true` | `false` | 标识本次调用是否成功 |
| `Data` | `T` | 反序列化后的对象 | `null` | 只在 IsSuccess=true 时有意义 |
| `StatusCode` | `HttpStatusCode?` | `200` 等 2xx | `400` / `401` / `404` / `422` / `500` 等 | HTTP 状态码 |
| `ErrorMsg` | `string?` | `null` | 全局错误信息（优先使用 `detail` 或首条 `Errors` 消息） | 由 `detail` 或首条 `Errors` 消息生成；`Errors` 列表仍保留所有项；解析失败则为原始响应文本 |
| `Errors` | `IEnumerable<Error>` | 空集合 | 错误详情列表 | Code 和 Message 字段的含义取决于错误类型（字段/业务错误/自定义） |

### 调用方法快速参考

```csharp
// 1. GET 请求（最简形式）
var result = await _httpClient.GetAsync<User>("api/users/123");

// 2. GET 请求（带查询参数）
var result = await _httpClient.GetAsync<IEnumerable<User>>(
    "api/users",
    queryParams: new { page = 1, pageSize = 10 }
);

// 3. POST 请求（带请求体）
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethod.Post,
    requestBody: new { name = "张三", email = "zhangsan@example.com" }
);

// 4. PUT 请求（带请求体）
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethod.Put,
    requestBody: new { name = "李四", email = "lisi@example.com" }
);

// 5. DELETE 请求
var result = await _httpClient.CallApi<object>(
    "api/users/123",
    HttpMethod.Delete
);

// 6. 带调用方超时和取消令牌
using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));
var result = await _httpClient.GetAsync<User>(
    "api/users/123",
    cancellationToken: timeoutSource.Token);

// 7. 完整参数
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethod.Get,
    requestBody: null,
    queryParams: new { includeDetails = true },
    cancellationToken: ct
);
```
