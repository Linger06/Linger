using System.DirectoryServices.AccountManagement;
using Linger.Ldap.Contracts;
using Xunit;
using Moq;

namespace Linger.Ldap.ActiveDirectory.Tests;

public class LdapTests
{
    private readonly LdapConfig _validConfig;
    private readonly Mock<PrincipalContext> _mockPrincipalContext;

    public LdapTests()
    {
        _validConfig = new LdapConfig
        {
            Url = "ldap.test.com",
            Domain = "TEST",
            SearchBase = "DC=test,DC=com",
            SearchFilter = "(&(objectClass=user)(sAMAccountName={0}))",
            Security = true,
            Credentials = new LdapCredentials
            {
                BindDn = "testUser",
                BindCredentials = "testPass"
            }
        };

        _mockPrincipalContext = new Mock<PrincipalContext>();
    }

    [Fact]
    public void Constructor_WithValidConfig_CreatesInstance()
    {
        // Arrange & Act
        var ldap = new Ldap(_validConfig);

        // Assert
        Assert.NotNull(ldap);
    }

    [Fact]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Ldap(null!));
    }

    [Theory]
    [InlineData("testUser", "correctPassword", true)]
    [InlineData("testUser", "wrongPassword", false)]
    public void ValidateUser_ReturnsExpectedResult(string username, string password, bool expected)
    {
        // Arrange
        var ldap = new Ldap(_validConfig);
        AdUserInfo? userInfo;

        // Mock user principal for successful validation
        var mockUserPrincipal = new Mock<UserPrincipal>(MockBehavior.Strict, _mockPrincipalContext.Object);
        mockUserPrincipal.Setup(u => u.SamAccountName).Returns(username);
        mockUserPrincipal.Setup(u => u.DisplayName).Returns("Test User");

        // Act
        var result = ldap.ValidateUser(username, password, out userInfo);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.NotNull(userInfo);
            Assert.Equal(username, userInfo.SamAccountName);
        }
        else
        {
            Assert.Null(userInfo);
        }
    }

    [Fact]
    public void FindUser_WithValidUsername_ReturnsUserInfo()
    {
        // Arrange
        var username = "testUser";
        var ldap = new Ldap(_validConfig);

        // Act
        var result = ldap.FindUser(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.SamAccountName);
    }

    [Fact]
    public void FindUser_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentUser";
        var ldap = new Ldap(_validConfig);

        // Act
        var result = ldap.FindUser(username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUsers_WithValidPattern_ReturnsUsers()
    {
        // Arrange
        var searchPattern = "test";
        var ldap = new Ldap(_validConfig);

        // Act
        var results = ldap.GetUsers(searchPattern);

        // Assert
        Assert.NotNull(results);
        Assert.All(results, user => Assert.NotNull(user));
    }

    [Fact]
    public void GetUsers_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchPattern = "nonexistent";
        var ldap = new Ldap(_validConfig);

        // Act
        var results = ldap.GetUsers(searchPattern);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void GetEntryByUsername_WithValidUsername_ReturnsDirectoryEntry()
    {
        // Arrange
        var username = "testUser";
        var ldap = new Ldap(_validConfig);

        // Act & Assert
        Assert.Throws<PlatformNotSupportedException>(() => ldap.GetEntryByUsername(username));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FindUser_WithInvalidUsername_ThrowsArgumentException(string username)
    {
        // Arrange
        var ldap = new Ldap(_validConfig);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ldap.FindUser(username));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateUser_WithInvalidCredentials_ReturnsFalse(string username)
    {
        // Arrange
        var ldap = new Ldap(_validConfig);

        // Act
        var result = ldap.ValidateUser(username, "password", out var userInfo);

        // Assert
        Assert.False(result);
        Assert.Null(userInfo);
    }

    [Fact]
    public void GetUsers_WithCustomCredentials_UsesCorrectContext()
    {
        // Arrange
        var ldap = new Ldap(_validConfig);
        var credentials = new LdapCredentials
        {
            BindDn = "customUser",
            BindCredentials = "customPass"
        };

        // Act
        var results = ldap.GetUsers("test", credentials);

        // Assert
        Assert.NotNull(results);
    }
}

