using Xunit;
using Moq;
using Novell.Directory.Ldap;
using System.Collections;
using System.Collections.Generic;
using Linger.Ldap.Contracts;

namespace Linger.Ldap.Novell.Tests;

public class LdapTests2 : IDisposable
{
    private readonly LdapConfig _validConfig;
    private readonly Mock<ILdapConnection> _mockLdapConnection;
    private readonly Mock<ILdapSearchResults> _mockSearchResults;
    private readonly Ldap _ldap;

    public LdapTests2()
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
                BindDn = "cn=admin,dc=test,dc=com",
                BindCredentials = "testPass"
            }
        };

        _mockLdapConnection = new Mock<ILdapConnection>();
        _mockSearchResults = new Mock<ILdapSearchResults>();
        _ldap = new Ldap(_validConfig);
    }

    [Fact]
    public void Constructor_WithValidConfig_CreatesInstance()
    {
        // Arrange & Act
        var ldap = new Ldap(_validConfig);

        // Assert
        Assert.NotNull(ldap);
        Assert.True(ldap.IsConnected() == false);
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
    public void ValidateUser_WithCredentials_ReturnsExpectedResult(string username, string password, bool expected)
    {
        // Arrange
        SetupMockConnection(expected);
        var mockEntry = new Mock<LdapEntry>();
        mockEntry.Setup(e => e.GetAttributeSet())
            .Returns(new LdapAttributeSet());

        _mockSearchResults.Setup(r => r.Next()).Returns(mockEntry.Object);

        // Act
        var result = _ldap.ValidateUser(username, password, out var userInfo);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.NotNull(userInfo);
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
        SetupMockConnection(true);
        var mockEntry = CreateMockLdapEntry();
        _mockSearchResults.Setup(r => r.Next()).Returns(mockEntry);

        // Act
        var result = _ldap.FindUser("testUser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result.DisplayName);
    }

    [Fact]
    public void FindUser_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        SetupMockConnection(true);
        _mockSearchResults.Setup(r => r.Next()).Returns((LdapEntry)null!);

        // Act
        var result = _ldap.FindUser("nonexistentUser");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUsers_WithValidPattern_ReturnsUsers()
    {
        // Arrange
        SetupMockConnection(true);
        var mockEntries = new List<LdapEntry> { CreateMockLdapEntry(), CreateMockLdapEntry() };
        SetupMockSearchResults(mockEntries);

        // Act
        var results = _ldap.GetUsers("test");

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public void GetUsers_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        SetupMockConnection(true);
        SetupMockSearchResults(new List<LdapEntry>());

        // Act
        var results = _ldap.GetUsers("nonexistent");

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void Connect_WithValidCredentials_ReturnsTrue()
    {
        // Arrange
        SetupMockConnection(true);

        // Act
        var result = _ldap.Connect(new LdapCredentials { BindDn = "testUser", BindCredentials = "testPass" });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Connect_WithInvalidCredentials_ReturnsFalse()
    {
        // Arrange
        _mockLdapConnection.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new LdapException());

        // Act
        var result = _ldap.Connect(new LdapCredentials { BindDn = "invalid", BindCredentials = "invalid" });

        // Assert
        Assert.False(result);
    }

    private void SetupMockConnection(bool isConnected)
    {
        _mockLdapConnection.Setup(c => c.Connected).Returns(isConnected);
        _mockLdapConnection.Setup(c => c.Search(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string[]>(),
            It.IsAny<bool>()
        )).Returns(_mockSearchResults.Object);
    }

    private void SetupMockSearchResults(IList<LdapEntry> entries)
    {
        var enumerator = entries.GetEnumerator();
        _mockSearchResults.Setup(r => r.HasMore()).Returns(() => enumerator.MoveNext());
        _mockSearchResults.Setup(r => r.Next()).Returns(() => (LdapEntry)enumerator.Current);
    }

    private static LdapEntry CreateMockLdapEntry()
    {
        var attributes = new LdapAttributeSet();
        attributes.Add(new LdapAttribute("displayName", "Test User"));
        attributes.Add(new LdapAttribute("mail", "test@test.com"));
        return new LdapEntry("cn=Test User,dc=test,dc=com", attributes);
    }

    public void Dispose()
    {
        _ldap?.DisConnect();
    }
}

