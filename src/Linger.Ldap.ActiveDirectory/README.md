# Linger.Ldap.ActiveDirectory

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

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET 6.0
- .NET Standard 2.0

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

// Authenticate with default SearchBase
var (isValid, userInfo) = await ldap.ValidateUserAsync("username", "password");
if (isValid && userInfo != null) 
{
    Console.WriteLine($"User authenticated: {userInfo.DisplayName}"); 
    Console.WriteLine($"Email: {userInfo.Email}"); 
    Console.WriteLine($"Department: {userInfo.Department}"); 
}

// Authenticate in specific OU
var (isValid2, userInfo2) = await ldap.ValidateUserAsync(
    "username", 
    "password",
    searchBase: "OU=Sales,DC=company,DC=com"
);
```

### Finding Users
```csharp
using var ldap = new Ldap(config);

// Find specific user in default SearchBase
var user = await ldap.FindUserAsync("username"); 
if (user != null) 
{ 
    Console.WriteLine($"Name: {user.DisplayName}"); 
    Console.WriteLine($"Email: {user.Email}"); 
    Console.WriteLine($"Title: {user.Title}"); 
}

// Find user in specific OU
var salesUser = await ldap.FindUserAsync(
    "username",
    searchBase: "OU=Sales,DC=company,DC=com"
);

// Search users with pattern
var users = await ldap.GetUsersAsync("john*"); 
foreach (var foundUser in users) 
{ 
    Console.WriteLine($"Found: {foundUser.DisplayName}");
    Console.WriteLine($"Groups: {string.Join(", ", foundUser.MemberOf ?? Array.Empty<string>())}"); 
}

// Search users in specific OU
var itUsers = await ldap.GetUsersAsync(
    "john*",
    searchBase: "OU=IT,DC=company,DC=com"
);
```

### Flexible OU Search (New in v1.0)
```csharp
// Search across multiple OUs
string[] organizationalUnits = 
{
    "OU=Sales,DC=company,DC=com",
    "OU=IT,DC=company,DC=com",
    "OU=HR,DC=company,DC=com"
};

AdUserInfo? foundUser = null;
foreach (var ou in organizationalUnits)
{
    foundUser = await ldap.FindUserAsync("john.doe", searchBase: ou);
    if (foundUser != null)
    {
        Console.WriteLine($"User found in: {ou}");
        break;
    }
}

// Check user existence in specific OU
bool exists = await ldap.UserExistsAsync(
    "username",
    searchBase: "OU=Contractors,DC=company,DC=com"
);
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

