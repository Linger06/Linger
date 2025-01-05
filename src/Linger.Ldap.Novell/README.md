# Linger.Ldap.Novell

A comprehensive .NET library providing seamless integration with LDAP directories using the Novell.Directory.Ldap provider, with cross-platform support.

## Features

### Core Functionality
- Platform-independent LDAP operations
- SSL/TLS secure connections
- Connection pooling and management
- Comprehensive error handling

### User Management
- User authentication and validation
- Detailed user information retrieval
- Advanced search capabilities
- Group membership querying

### Information Categories
- Basic identification (username, display name, UPN)
- Personal information (first name, last name, initials)
- Contact details (email, phone numbers, addresses)
- Organization info (department, title, employee ID)
- System attributes (workstations, profile paths)
- Security settings (account status, password info)

## Supported Frameworks

- .NET 9.0
- .NET 8.0

## Installation
### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.Ldap.Novell".
5. Click on the `Linger.Ldap.Novell` package, select the appropriate version and click Install.

### Package Manager Console

```
PM> Install-Package Linger.Ldap.Novell
```

### .NET CLI Console

```
> dotnet add package Linger.Ldap.Novell
```

## Usage Examples

### Basic Configuration
```csharp
var config = new LdapConfig 
{ 
    Url = "ldap.company.com", 
    Domain = "COMPANY", 
    SearchBase = "DC=company,DC=com", 
    Security = true, 
    Credentials = new LdapCredentials { BindDn = "serviceAccount", BindCredentials = "password" } 
};
```

### User Authentication
```csharp
using var ldap = new Ldap(config); 
if (ldap.ValidateUser("username", "password", out var userInfo)) 
{
    Console.WriteLine($"User authenticated: {userInfo.DisplayName}"); 
    Console.WriteLine($"Email: {userInfo.Email}"); 
    Console.WriteLine($"Department: {userInfo.Department}"); 
}
```

### Finding Users
```csharp
using var ldap = new Ldap(config);

// Find specific user 
var user = ldap.FindUser("username"); 
if (user != null) 
{ 
    Console.WriteLine($"Name: {user.DisplayName}"); 
    Console.WriteLine($"Email: {user.Email}"); 
    Console.WriteLine($"Title: {user.Title}"); 
}

// Search users with pattern
var users = ldap.GetUsers("john*"); 
foreach (var foundUser in users) 
{ 
    Console.WriteLine($"Found: {foundUser.DisplayName}");
    Console.WriteLine($"Groups: {string.Join(", ", foundUser.MemberOf ?? Array.Empty())}"); 
}
```

## Available User Properties

### Identification
- DisplayName
- SamAccountName
- UserPrincipalName (UPN)
- DistinguishedName (DN)

### Personal Information
- FirstName
- LastName
- Description
- Initials

### Contact Information
- Email
- TelephoneNumber
- Mobile
- HomePhone
- Fax
- IpPhone
- WebPage

### Organization Details
- Company
- Department
- Title
- Manager
- EmployeeId
- EmployeeNumber

### Address Information
- Street
- City
- State
- PostalCode
- Country
- PostOfficeBox

### System Information
- UserWorkstations
- ProfilePath
- HomeDrive
- HomeDirectory
- WhenCreated

### Security Information
- Status (Enabled/Disabled/Locked/Expired)
- AccountExpires
- PwdLastSet
- PwdExpirationLeftDays
- MemberOf (Group memberships)

## Key Differences from Active Directory Version

- Cross-platform support (Windows, Linux, macOS)
- Different connection handling mechanism
- Platform-independent authentication
- Native SSL/TLS support
- More flexible LDAP server compatibility

## Requirements

- LDAP/LDAPS server access
- Appropriate LDAP permissions

## Contributing

We welcome contributions! Please:

1. Fork the repository
2. Create a feature branch
3. Submit a Pull Request

## License

This project is licensed under the MIT License.
