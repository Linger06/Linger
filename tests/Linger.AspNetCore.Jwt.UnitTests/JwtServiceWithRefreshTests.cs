using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt.UnitTests;

[Collection(JwtEnvironmentCollection.Name)]
public class JwtServiceWithRefreshTests
{
    [Fact]
    public async Task RefreshTokenAsync_WhenSameRefreshTokenIsUsedConcurrently_AllowsOnlyOneRotation()
    {
        using var environmentScope = new SecretEnvironmentScope();
        var service = new ConcurrentRotationJwtService();
        var original = await service.CreateTokenAsync("user-1");

        var attempts = await Task.WhenAll(
            TryRefreshAsync(service, original),
            TryRefreshAsync(service, original));

        var success = Assert.Single(attempts.Where(attempt => attempt.Token is not null));
        var failure = Assert.Single(attempts.Where(attempt => attempt.Exception is not null));
        Assert.True(success.Token!.HasRefreshToken);
        Assert.IsType<SecurityTokenException>(failure.Exception);
    }

    private static async Task<RefreshAttempt> TryRefreshAsync(
        ConcurrentRotationJwtService service,
        Token token)
    {
        try
        {
            return new RefreshAttempt(await service.RefreshTokenAsync(token), null);
        }
        catch (SecurityTokenException ex)
        {
            return new RefreshAttempt(null, ex);
        }
    }

    private sealed record RefreshAttempt(Token? Token, SecurityTokenException? Exception);

    private sealed class ConcurrentRotationJwtService : JwtServiceWithRefresh
    {
        private readonly object _gate = new();
        private readonly TaskCompletionSource<bool> _rotationAttemptsReady = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private JwtRefreshToken? _storedToken;
        private int _rotationAttempts;

        internal ConcurrentRotationJwtService() : base(JwtTestData.CreateOptions())
        {
        }

        protected override Task StoreRefreshTokenAsync(
            string userId,
            JwtRefreshToken refreshToken,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                _storedToken = refreshToken;
            }

            return Task.CompletedTask;
        }

        protected override async Task<bool> TryRotateRefreshTokenAsync(
            string userId,
            string presentedRefreshToken,
            JwtRefreshToken replacement,
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _rotationAttempts) == 2)
            {
                _rotationAttemptsReady.TrySetResult(true);
            }

            await _rotationAttemptsReady.Task.WaitAsync(cancellationToken);
            lock (_gate)
            {
                if (_storedToken is null ||
                    _storedToken.ExpiryTime <= DateTime.UtcNow ||
                    !string.Equals(
                        _storedToken.RefreshToken,
                        presentedRefreshToken,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                _storedToken = replacement;

                return true;
            }
        }
    }
}
