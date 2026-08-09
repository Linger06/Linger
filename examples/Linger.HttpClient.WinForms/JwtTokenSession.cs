using Linger.AspNetCore.Jwt.Contracts;
using Linger.HttpClient.Standard;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Linger.HttpClient.WinForms;

internal sealed class JwtTokenSession(StandardHttpClient refreshClient)
{
    private static readonly TimeSpan s_refreshAhead = TimeSpan.FromMinutes(1);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private TokenState? _current;

    public void SetToken(Token token, Uri refreshEndpoint)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(refreshEndpoint);

        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new ArgumentException(AppResources.AccessTokenRequired, nameof(token));
        }

        if (!token.HasRefreshToken)
        {
            throw new ArgumentException(AppResources.RefreshTokenRequired, nameof(token));
        }

        var tokenCopy = new Token(token.AccessToken, token.RefreshToken);
        Volatile.Write(ref _current, new TokenState(tokenCopy, refreshEndpoint));
    }

    public void Clear()
    {
        Volatile.Write(ref _current, null);
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken)
    {
        var current = GetCurrent();
        if (!ShouldRefresh(current.Token.AccessToken))
        {
            return current.Token.AccessToken;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            current = GetCurrent();
            if (!ShouldRefresh(current.Token.AccessToken))
            {
                return current.Token.AccessToken;
            }

            var result = await refreshClient.CallApi<Token>(
                current.RefreshEndpoint.AbsoluteUri,
                HttpMethod.Post,
                requestBody: current.Token,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess ||
                result.Data is null ||
                string.IsNullOrWhiteSpace(result.Data.AccessToken) ||
                !result.Data.HasRefreshToken)
            {
                throw new SecurityTokenException(result.ErrorMsg ?? AppResources.RefreshFailed);
            }

            var refreshed = new TokenState(
                new Token(result.Data.AccessToken, result.Data.RefreshToken),
                current.RefreshEndpoint);
            Volatile.Write(ref _current, refreshed);

            return refreshed.Token.AccessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private TokenState GetCurrent()
    {
        return Volatile.Read(ref _current)
            ?? throw new InvalidOperationException(AppResources.TokenSessionRequired);
    }

    private static bool ShouldRefresh(string accessToken)
    {
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);

        return jwt.ValidTo <= DateTime.UtcNow.Add(s_refreshAhead);
    }

    private sealed record TokenState(Token Token, Uri RefreshEndpoint);
}
