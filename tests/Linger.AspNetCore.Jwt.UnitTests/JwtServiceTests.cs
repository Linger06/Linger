using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt.UnitTests;

[Collection(JwtEnvironmentCollection.Name)]
public class JwtServiceTests
{
    [Fact]
    public void Constructor_WhenIssuerIsMissing_ThrowsInvalidOperationException()
    {
        using var environmentScope = new SecretEnvironmentScope();
        var options = JwtTestData.CreateOptions(issuer: string.Empty);

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtService(options));

        Assert.Contains(nameof(JwtOption.Issuer), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenAudienceIsMissing_ThrowsInvalidOperationException()
    {
        using var environmentScope = new SecretEnvironmentScope();
        var options = JwtTestData.CreateOptions(audience: string.Empty);

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtService(options));

        Assert.Contains(nameof(JwtOption.Audience), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenSecurityKeyIsTooShort_ThrowsInvalidOperationException()
    {
        using var environmentScope = new SecretEnvironmentScope();
        var options = JwtTestData.CreateOptions(securityKey: "too-short");

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtService(options));

        Assert.Contains("32 bytes", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenOptionsAreValid_CreatesValidSignedToken()
    {
        using var environmentScope = new SecretEnvironmentScope();
        var options = JwtTestData.CreateOptions();
        var service = new JwtService(options);

        var token = await service.CreateTokenAsync("user-1");

        var validationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.Name,
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecurityKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token.AccessToken,
            validationParameters,
            out var validatedToken);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

        Assert.Equal("user-1", principal.Identity?.Name);
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
    }
}

[CollectionDefinition("JWT environment", DisableParallelization = true)]
public sealed class JwtEnvironmentCollection
{
    public const string Name = "JWT environment";
}

internal static class JwtTestData
{
    private const string Issuer = "https://issuer.example.test";
    private const string Audience = "https://audience.example.test";
    internal const string SecurityKey = "0123456789abcdef0123456789abcdef";

    internal static JwtOption CreateOptions(
        string issuer = Issuer,
        string audience = Audience,
        string securityKey = SecurityKey)
    {
        return new JwtOption
        {
            Issuer = issuer,
            Audience = audience,
            SecurityKey = securityKey,
            ExpiresInMinutes = 15,
            RefreshTokenExpiresInMinutes = 60
        };
    }
}

internal sealed class SecretEnvironmentScope : IDisposable
{
    private const string VariableName = "SECRET";
    private readonly string? _originalValue = Environment.GetEnvironmentVariable(VariableName);

    internal SecretEnvironmentScope()
    {
        Environment.SetEnvironmentVariable(VariableName, null);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(VariableName, _originalValue);
    }
}
