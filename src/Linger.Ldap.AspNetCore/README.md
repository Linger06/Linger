# Linger.Ldap.AspNetCore

Linger.Ldap.AspNetCore is a C# helper library designed to simplify LDAP operations in ASP.NET Core applications.

## Features

- Easy integration with ASP.NET Core
- Simplified LDAP operations
- Configuration options via `Microsoft.Extensions.Options.ConfigurationExtensions`

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later

### Installation

#### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.Ldap.AspNetCore".
5. Click on the `Linger.Ldap.AspNetCore` package, select the appropriate version and click Install.

#### Package Manager Console

```
PM> Install-Package Linger.Ldap.AspNetCore
```

#### .NET CLI Console

```
> dotnet add package Linger.Ldap.AspNetCore
```

### Usage

1. Add the necessary configuration in your `appsettings.json`:
    ```json
    "LdapConfig": {
        "Url": "ldap://your-ldap-server",
        "Security": false,
        "Domain": "your-domain",
        "Credentials": {
            "BindDn": "your-bind-dn",
            "BindCredentials": "your-bind-password"
        },
        "SearchBase": "DC=example,DC=com",
        "SearchFilter": "(&(objectClass=user)(objectClass=person)(sAMAccountName={0}))",
        "Attributes": [
            "memberOf",
            "displayName",
            "sAMAccountName",
            "userPrincipalName"
        ]
    }
    ```

2. Register the LDAP services in your `Program.cs`:
    ```csharp
    using Linger.Ldap.AspNetCore;
    builder.Services.AddLdapService(builder.Configuration);
    ```

3. Inject and use the `ILdapService` in your controllers or services:
    ```csharp
    public class HomeController : Controller
    {
        private readonly ILdapService _ldapService;

        public HomeController(ILdapService ldapService)
        {
            _ldapService = ldapService;
        }

        public IActionResult Index()
        {
            var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
            var users = _ldapService.FindUser(userName,ldapCredentials);
            return View(users);
        }
    }
    ```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.
