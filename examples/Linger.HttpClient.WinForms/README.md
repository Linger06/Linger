# Linger.HttpClient WinForms Example

This project demonstrates direct `StandardHttpClient` construction in WinForms and automatic JWT attachment and refresh through a `DelegatingHandler`. The complete implementation is available in the project's `JwtTokenSession`, `AccessTokenHandler`, and form initialization code.

## Configure trusted origins and run

In PowerShell at the repository root, replace these two example origins with the HTTPS services you trust, then start the example from the **same shell**:

```powershell
$env:LINGER_WINFORMS_API_ORIGIN = 'https://localhost:7141'
$env:LINGER_WINFORMS_REFRESH_ORIGIN = 'https://localhost:7242'
dotnet run --project examples/Linger.HttpClient.WinForms/Linger.HttpClient.WinForms.csproj
```

- Set each variable independently, even when both services share an origin. Use only `https://host[:port]`, optionally with a trailing slash; do not include credentials, a path, a query or a fragment. The server must present a trusted TLS certificate. A missing or invalid setting blocks that client; restart after changing either value.
- With the sample values, enter `https://localhost:7242/refresh` as the refresh endpoint, click **Apply tokens**, then send a GET to `https://localhost:7141/users/me`. Replace both paths with real routes. Editing either URL never changes which origin is trusted.
- The API pipeline checks the actual request URI before obtaining a bearer token. The refresh pipeline independently checks its POST destination before sending the token pair. The HTTPS host and effective port must match exactly; trust covers **all paths** on that origin. Do not trust a host that serves untrusted applications. Neither client follows redirects (including 307/308); enter the final HTTPS endpoint directly.

Expose login and refresh endpoints as described in the [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.md). After login succeeds, enter the returned access token, refresh token, refresh endpoint, and absolute API request URL in the form.

Use the selector at the top of the form to switch between Chinese and English. The selection immediately updates the UI resources and applies the selected culture name as the `Accept-Language` header for API and refresh-token requests.

For each request:

1. `AccessTokenHandler` obtains a valid access token from `JwtTokenSession`.
2. The access token is refreshed one minute before expiration.
3. `SemaphoreSlim` ensures concurrent requests perform only one refresh.
4. A successful refresh stores both the new access token and the rotated refresh token.
5. A failed refresh clears the session and requires another sign-in.

The refresh operation uses a separate `HttpClient` and cannot recursively enter `AccessTokenHandler`. Arbitrary `401` responses are not retried, and non-repeatable requests such as upload streams are never replayed.
