# Linger.AspNetCore.Jwt.Contracts

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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
    /// <returns>Token object containing the access token</returns>
    Task<Token> CreateTokenAsync(string userId);
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
    /// <returns>New token object</returns>
    Task<Token> RefreshTokenAsync(Token token);
}
```

## Related Packages

- [Linger.AspNetCore.Jwt](../Linger.AspNetCore.Jwt/) - Implementation of these contracts
