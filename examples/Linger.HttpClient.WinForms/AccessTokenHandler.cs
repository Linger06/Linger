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
