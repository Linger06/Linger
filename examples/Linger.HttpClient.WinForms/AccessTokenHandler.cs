using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Linger.HttpClient.WinForms;

internal sealed class AccessTokenHandler(JwtTokenSession tokenSession) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenSession
            .GetValidAccessTokenAsync(cancellationToken)
            .ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

// 信任配置只在创建客户端时读取，不能由窗体中的请求地址更新。
internal sealed class TrustedOriginHandler(string variable, Func<string> validationError, HttpMessageHandler innerHandler)
    : DelegatingHandler(innerHandler)
{
    private readonly Uri? _origin = ReadOrigin(variable);

    internal bool TryGetTarget(string value, [NotNullWhen(true)] out Uri? target)
    {
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out target) && Allows(target);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 检查 HttpClient 已解析的最终 URI；业务请求在获取令牌（可能触发刷新）之前即被拦截。
        if (!Allows(request.RequestUri) || request.Headers.Host is not null)
        {
            throw new InvalidOperationException(validationError());
        }

        return base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 示例仅实现异步令牌管线，不允许通过同步入口绕过检查。
        throw new NotSupportedException();
    }

    private bool Allows(Uri? target)
    {
        return _origin is not null && target is { IsAbsoluteUri: true } &&
            target.Scheme == Uri.UriSchemeHttps && target.UserInfo.Length == 0 && target.Fragment.Length == 0 &&
            target.Port == _origin.Port && string.Equals(target.IdnHost, _origin.IdnHost, StringComparison.OrdinalIgnoreCase);
    }

    private static Uri? ReadOrigin(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable)?.Trim();
        if (!Uri.TryCreate(value, UriKind.Absolute, out var origin) || !origin.IsWellFormedOriginalString() ||
            origin.Scheme != Uri.UriSchemeHttps || origin.UserInfo.Length != 0 || origin.PathAndQuery != "/" || origin.Fragment.Length != 0)
        {
            return null;
        }

        // 不把带路径的地址规范化为可信源（例如 /foo/..）。只允许可选的末尾斜杠。
        var slash = value.IndexOf('/', Uri.UriSchemeHttps.Length + Uri.SchemeDelimiter.Length);

        return slash < 0 || slash == value.Length - 1 ? origin : null;
    }
}
