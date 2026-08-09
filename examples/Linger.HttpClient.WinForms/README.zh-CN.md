# Linger.HttpClient WinForms 示例

该项目展示 WinForms 客户端直接创建 `StandardHttpClient`，并通过 `DelegatingHandler` 自动添加和刷新 JWT。完整实现位于项目源码的 `JwtTokenSession`、`AccessTokenHandler` 和窗体初始化代码中。

## 运行

```powershell
dotnet run --project examples/Linger.HttpClient.WinForms/Linger.HttpClient.WinForms.csproj
```

服务端需要按照 [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.zh-CN.md) 暴露登录和刷新端点。登录成功后，将返回的 Access Token、Refresh Token、刷新端点和需要调用的完整 API 地址填入窗体。

窗体顶部可以在中文和英文之间切换。切换后会立即刷新界面资源，并将所选区域性的名称作为 `Accept-Language` 请求头应用到 API 请求和 Refresh Token 请求。

发送请求时：

1. `AccessTokenHandler` 从 `JwtTokenSession` 取得有效的 Access Token。
2. Access Token 将在过期前一分钟通过刷新端点更新。
3. `SemaphoreSlim` 保证并发请求只执行一次刷新。
4. 刷新成功后同时保存新的 Access Token 和轮换后的 Refresh Token。
5. 刷新失败时清除当前会话，并要求重新登录。

刷新请求使用独立的 `HttpClient`，不会递归进入 `AccessTokenHandler`。普通 `401` 不会被自动重试，上传流等不可重复请求也不会被重放。

## 关键实现

### Token 会话

`JwtTokenSession` 保存登录或刷新端点返回的完整 `Token`。每次请求发送前检查 Access Token 的有效期，在到期前一分钟刷新：

```csharp
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtTokenSession : IDisposable
{
    private static readonly TimeSpan RefreshAhead = TimeSpan.FromMinutes(1);
    private readonly StandardHttpClient _refreshClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Token _current;

    public JwtTokenSession(StandardHttpClient refreshClient, Token initialToken)
    {
        if (!initialToken.HasRefreshToken)
        {
            throw new ArgumentException("A refresh token is required.", nameof(initialToken));
        }

        _refreshClient = refreshClient;
        _current = initialToken;
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken)
    {
        var current = Volatile.Read(ref _current);
        if (!ShouldRefresh(current.AccessToken))
        {
            return current.AccessToken;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            current = Volatile.Read(ref _current);
            if (!ShouldRefresh(current.AccessToken))
            {
                return current.AccessToken;
            }

            var result = await _refreshClient.CallApi<Token>(
                "refresh",
                HttpMethod.Post,
                requestBody: current,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess || result.Data is null || !result.Data.HasRefreshToken)
            {
                throw new SecurityTokenException(
                    result.ErrorMsg ?? "The refresh token is invalid or expired.");
            }

            Volatile.Write(ref _current, result.Data);
            return result.Data.AccessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static bool ShouldRefresh(string accessToken)
    {
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);

        return jwt.ValidTo <= DateTime.UtcNow.Add(RefreshAhead);
    }

    public void Dispose()
    {
        _refreshLock.Dispose();
    }
}
```

会话对象需要在窗体关闭时释放其 `SemaphoreSlim`。刷新失败时应清除本地令牌并返回登录界面。

### 请求处理器

处理器只负责取得有效令牌并写入当前请求，不修改共享的 `DefaultRequestHeaders`：

```csharp
public sealed class AccessTokenHandler(JwtTokenSession tokenSession) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenSession
            .GetValidAccessTokenAsync(cancellationToken)
            .ConfigureAwait(false);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
```

### 双客户端初始化

刷新端点使用不带 `AccessTokenHandler` 的独立客户端，避免刷新请求递归进入处理器：

```csharp
_refreshHttpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseAddress)
};
var refreshClient = new StandardHttpClient(_refreshHttpClient);
_tokenSession = new JwtTokenSession(refreshClient, loginToken);

var accessTokenHandler = new AccessTokenHandler(_tokenSession)
{
    InnerHandler = new HttpClientHandler()
};
_httpClient = new HttpClient(accessTokenHandler)
{
    BaseAddress = new Uri(apiBaseAddress)
};
_client = new StandardHttpClient(_httpClient);
```

应用关闭时依次释放 `_httpClient`、`_refreshHttpClient` 和 `_tokenSession`。完整服务端配置参见 [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.zh-CN.md)。
