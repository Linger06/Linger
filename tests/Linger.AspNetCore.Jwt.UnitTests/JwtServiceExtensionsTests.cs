using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt.UnitTests;

[Collection(JwtEnvironmentCollection.Name)]
public class JwtServiceExtensionsTests
{
    private static readonly Token s_token = new("access-token", "refresh-token");

    [Fact]
    public async Task RefreshTokenResultAsync_WhenTokenArgumentIsInvalid_ReturnsFailure()
    {
        var service = new StubRefreshableJwtService((_, _) =>
            Task.FromException<Token>(new ArgumentException("Invalid token.", "token")));

        var result = await service.RefreshTokenResultAsync(s_token);

        Assert.False(result.Success);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task RefreshTokenResultAsync_WhenStorageThrowsArgumentException_PropagatesException()
    {
        var service = new StubRefreshableJwtService((_, _) =>
            Task.FromException<Token>(new ArgumentException("Invalid storage key.", "storageKey")));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.RefreshTokenResultAsync(s_token));

        Assert.Equal("storageKey", exception.ParamName);
    }

    [Fact]
    public async Task RefreshTokenResultAsync_WhenOperationIsCanceled_PropagatesCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var service = new StubRefreshableJwtService((_, cancellationToken) =>
            Task.FromCanceled<Token>(cancellationToken));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.RefreshTokenResultAsync(s_token, cancellationSource.Token));
    }

    private sealed class StubRefreshableJwtService(
        Func<Token, CancellationToken, Task<Token>> refresh) : IRefreshableJwtService
    {
        public Task<Token> CreateTokenAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Token("access-token"));
        }

        public Task<Token> RefreshTokenAsync(
            Token token,
            CancellationToken cancellationToken = default)
        {
            return refresh(token, cancellationToken);
        }
    }
}
