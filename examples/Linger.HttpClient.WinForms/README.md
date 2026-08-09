# Linger.HttpClient WinForms Example

This project demonstrates direct `StandardHttpClient` construction in WinForms and automatic JWT attachment and refresh through a `DelegatingHandler`. The complete implementation is available in the project's `JwtTokenSession`, `AccessTokenHandler`, and form initialization code.

## Run

```powershell
dotnet run --project examples/Linger.HttpClient.WinForms/Linger.HttpClient.WinForms.csproj
```

Expose login and refresh endpoints as described in the [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.md). After login succeeds, enter the returned access token, refresh token, refresh endpoint, and absolute API request URL in the form.

Use the selector at the top of the form to switch between Chinese and English. The selection immediately updates the UI resources and applies the selected culture name as the `Accept-Language` header for API and refresh-token requests.

For each request:

1. `AccessTokenHandler` obtains a valid access token from `JwtTokenSession`.
2. The access token is refreshed one minute before expiration.
3. `SemaphoreSlim` ensures concurrent requests perform only one refresh.
4. A successful refresh stores both the new access token and the rotated refresh token.
5. A failed refresh clears the session and requires another sign-in.

The refresh operation uses a separate `HttpClient` and cannot recursively enter `AccessTokenHandler`. Arbitrary `401` responses are not retried, and non-repeatable requests such as upload streams are never replayed.

## Key implementation

### Token session

`JwtTokenSession` retains the complete `Token` returned by login or refresh. It checks the access token before each request and refreshes it one minute before expiration:

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

Dispose the session with the form so its `SemaphoreSlim` is released. When refresh fails, clear the local tokens and return to sign-in.

### Request handler

The handler obtains a valid token and writes it only to the current request. It does not mutate shared `DefaultRequestHeaders`:

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

### Two-client initialization

Use a separate client without `AccessTokenHandler` for refresh requests so that refreshing cannot recursively enter the handler:

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

Dispose `_httpClient`, `_refreshHttpClient`, and `_tokenSession` when the application closes. See the [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.md) for complete server configuration.
