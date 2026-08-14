# Linger.AspNetCore.Jwt.Contracts

> Breaking changes and 2.0 migration notes are documented in the [Linger migration guide](../Linger/MIGRATION.md).

Core interfaces and abstractions for JWT (JSON Web Token) authentication and authorization in ASP.NET Core applications.

## Features

- Standardized interfaces for JWT token generation and validation
- Support for refresh token functionality
- Clean separation between contracts and implementation
- JWT configuration options model
- Extension methods for service registration

## Supported Frameworks

- .NET 8.0+
- .NET 9.0+

## Installation

```shell
dotnet add package Linger.AspNetCore.Jwt.Contracts
```

## Core Interfaces

### IJwtService

```csharp
public interface IJwtService
{
    /// <summary>
    /// Creates a JWT token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token object containing the access token</returns>
    Task<Token> CreateTokenAsync(string userId, CancellationToken cancellationToken = default);
}
```

### IRefreshableJwtService

```csharp
public interface IRefreshableJwtService : IJwtService
{
    /// <summary>
    /// Refreshes a JWT token
    /// </summary>
    /// <param name="token">Token object containing access token and refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New token object</returns>
    Task<Token> RefreshTokenAsync(Token token, CancellationToken cancellationToken = default);
}
```

`Linger.AspNetCore.Jwt.JwtServiceWithRefresh` separates initial storage from rotation. Derived services implement `StoreRefreshTokenAsync` for first issuance and `TryRotateRefreshTokenAsync` for refresh. The latter must atomically compare the presented token and replace it in the storage layer; never implement rotation as a read followed by a write, or concurrent refreshes can both succeed.

## Related Packages

- [Linger.AspNetCore.Jwt](../Linger.AspNetCore.Jwt/) - Implementation of these contracts
