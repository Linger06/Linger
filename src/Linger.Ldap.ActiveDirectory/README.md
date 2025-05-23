# Linger.Ldap.ActiveDirectory

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A comprehensive .NET library for Active Directory LDAP operations, providing simplified access to AD user information and authentication.

## Features

### User Management
- User authentication and validation
- Detailed user information retrieval
- User search capabilities
- Group membership information

### User Information Categories
- Basic identification (username, display name, UPN)
- Personal information (first name, last name, initials)
- Contact details (email, phone numbers, addresses)
- Organization info (department, title, employee ID)
- System attributes (workstations, profile paths)
- Security settings (account status, password info)


## Supported Frameworks
- .NET 9.0
- .NET 8.0
- .NET 6.0
- .NET Standard 2.0
- .NET Framework 4.6.2

## Installation

#### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.Ldap.ActiveDirectory".
5. Click on the `Linger.Ldap.ActiveDirectory` package, select the appropriate version and click Install.

#### Package Manager Console

```
PM> Install-Package Linger.Ldap.ActiveDirectory
```

#### .NET CLI Console

```
> dotnet add package Linger.Ldap.ActiveDirectory
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

## Requirements

- Windows operating system (uses System.DirectoryServices.AccountManagement)
- Appropriate Active Directory permissions

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Submit a Pull Request

## License

This project is licensed under the MIT License.

