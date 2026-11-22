using System.DirectoryServices;
using System.Security.Principal;
using Moq;
using Novell.Directory.Ldap;
using Linger.Ldap.ActiveDirectory;
using Linger.Ldap.Contracts;
using Linger.Ldap.Novell;
using Microsoft.Extensions.Options;

namespace Linger.Ldap.UnitTests;

public class LdapClientConfigurationTests
{
    [Fact]
    public void AdLdapClient_ImplementsSynchronousActiveDirectoryContract()
    {
        Assert.True(typeof(IActiveDirectoryClient).IsAssignableFrom(typeof(AdLdapClient)));
    }

    [Fact]
    public void AdLdapClient_DoesNotImplementAsynchronousLdapContract()
    {
        Assert.False(typeof(ILdapClient).IsAssignableFrom(typeof(AdLdapClient)));
    }

    [Fact]
    public void ActiveDirectoryContract_ContainsOnlySynchronousOperations()
    {
        var methods = typeof(IActiveDirectoryClient).GetMethods();

        Assert.All(methods, method =>
        {
            Assert.DoesNotContain("Async", method.Name, StringComparison.Ordinal);
            Assert.False(typeof(Task).IsAssignableFrom(method.ReturnType));
        });
    }

    [Fact]
    public void AdParameterlessConstructor_CreatesClientWithoutDiscoveringDomain()
    {
        var client = new AdLdapClient();

        Assert.NotNull(client);
    }

    [Fact]
    public void NovellConstructor_WithOptions_CreatesClient()
    {
        var options = Options.Create(CreateConfig());

        var client = new NovellLdapClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void AdConstructor_WithOptions_CreatesClient()
    {
        var options = Options.Create(CreateConfig());

        var client = new AdLdapClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void LdapConfigCopyConstructor_CreatesIndependentSnapshot()
    {
        var source = CreateConfig();
        source.MaxResults = 100;
        source.Attributes = ["mail"];
        source.Credentials = new LdapCredentials
        {
            BindDn = "reader",
            BindCredentials = "password"
        };

        var snapshot = new LdapConfig(source);
        source.MaxResults = 200;
        source.Attributes[0] = "displayName";
        source.Credentials.BindDn = "writer";

        Assert.Equal(100, snapshot.MaxResults);
        Assert.Equal("mail", Assert.Single(snapshot.Attributes!));
        Assert.Equal("reader", snapshot.Credentials!.BindDn);
    }

    [Theory]
    [InlineData("reader", null)]
    [InlineData(null, "password")]
    public void NovellConstructor_WithPartialCredentials_ThrowsArgumentException(string? bindDn, string? password)
    {
        var config = CreateConfig();
        config.Credentials = new LdapCredentials
        {
            BindDn = bindDn,
            BindCredentials = password
        };

        Assert.Throws<ArgumentException>(() => new NovellLdapClient(config));
    }

    [Fact]
    public void NovellConstructor_WithInvalidMaxResults_ThrowsArgumentOutOfRangeException()
    {
        var config = CreateConfig();
        config.MaxResults = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => new NovellLdapClient(config));
    }

    [Fact]
    public void LdapConfig_DoesNotForceLdapsOnActiveDirectory() => Assert.False(new LdapConfig().Security);

    [Fact]
    public void NovellConstructor_RejectsConfiguredPasswordWithoutTls()
    {
        var config = CreateConfig();
        config.Security = false;
        config.Credentials = new LdapCredentials { BindDn = "reader", BindCredentials = "password" };

        Assert.Throws<InvalidOperationException>(() => new NovellLdapClient(Options.Create(config)));
        Assert.NotNull(new AdLdapClient(config));
    }

    [Theory]
    [InlineData("credentials")]
    [InlineData("validate")]
    [InlineData("find")]
    [InlineData("get")]
    [InlineData("raw")]
    public async Task NovellClient_RejectsPasswordBeforeConnectAsync(string operation)
    {
        var config = CreateConfig();
        config.Security = false;
        var client = new NovellLdapClient(config);
        var credentials = new LdapCredentials { BindDn = "reader", BindCredentials = "password" };
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "credentials" => (Task)((ILdapCredentialValidator)client).ValidateCredentialsAsync("alice", "password", token),
            "validate" => client.ValidateUserAsync("alice", "password", cancellationToken: token),
            "find" => client.FindUserAsync("alice", credentials, cancellationToken: token),
            "get" => client.GetUsersAsync("alice", credentials, cancellationToken: token),
            _ => client.SearchUsersByFilterAsync("(objectClass=person)", credentials, cancellationToken: token)
        });
    }

    [Theory]
    [InlineData("alice", "sAMAccountName", "alice")]
    [InlineData(@"EXAMPLE\alice", "sAMAccountName", "alice")]
    [InlineData("alice@example.com", "userPrincipalName", "alice@example.com")]
    [InlineData(@"CN=Alice\, A,DC=example,DC=com", "distinguishedName", @"CN=Alice\, A,DC=example,DC=com")]
    public void AdIdentity_UsesOneExplicitIdentityType(string login, string type, string value)
    {
        var identity = AdLdapClient.ParseIdentity(login);

        Assert.Equal(type, identity.Attribute);
        Assert.Equal(value, identity.Value);
    }

    [Theory]
    [InlineData(@"OTHER\alice", @"EXAMPLE\alice", false)]
    [InlineData(@"EXAMPLE\alice", @"example\Alice", true)]
    [InlineData("alice", @"EXAMPLE\bob", false)]
    [InlineData("alice", @"EXAMPLE\alice", true)]
    [InlineData("alice", null, false)]
    public void AdIdentity_RequiresCanonicalDomain(string login, string? canonical, bool expected)
    {
        Assert.Equal(expected, AdLdapClient.CanValidateIdentity(login, canonical));
    }

    [Theory]
    [InlineData("u:alice", null)]
    [InlineData("", null)]
    [InlineData("dn:", null)]
    [InlineData(@"u:EXAMPLE\alice", "(sAMAccountName=alice)")]
    [InlineData("dn:CN=A*,OU=Users,DC=example", @"(|(entryDN=CN=A\2a,OU=Users,DC=example)(distinguishedName=CN=A\2a,OU=Users,DC=example))")]
    public void NovellIdentity_UsesOnlyServerAssertedNames(string authorizationId, string? expectedFilter)
    {
        Assert.Equal(expectedFilter, NovellLdapClient.GetIdentityFilter(authorizationId));
    }

    [Theory]
    [InlineData(true, AuthenticationTypes.SecureSocketsLayer)]
    [InlineData(false, AuthenticationTypes.Signing | AuthenticationTypes.Sealing)]
    public void AdIdentity_UsesOnlyExplicitSecureBinding(bool tls, AuthenticationTypes transport)
    {
        Assert.Equal(AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | transport, AdLdapClient.GetAuthenticationFlags(tls));
    }

    [Theory]
    [InlineData("dc01.example.test", true)]
    [InlineData("example.test", false)]
    [InlineData("dc02.example.test", false)]
    public void AdIdentity_RequiresExactController(string requested, bool expected)
    {
        Assert.Equal(expected, AdLdapClient.IsPinnedController(requested, "DC01.example.test"));
    }

    [Theory]
    [InlineData("S-1-5-21-1-2-3-1001", true)]
    [InlineData("S-1-5-21-4-5-6-1001", false)]
    public void AdIdentity_RequiresControllerDomainSid(string account, bool expected)
    {
        if (!OperatingSystem.IsWindows()) return;
        static byte[] EncodeSid(string value)
        {
            var sid = new SecurityIdentifier(value);
            var bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);

            return bytes;
        }

        Assert.Equal(expected, AdLdapClient.IsInControllerDomain(EncodeSid(account), EncodeSid("S-1-5-21-1-2-3")));
    }

    [Fact]
    public void AdIdentity_SearchDoesNotFollowReferralsOrLimitToFirstMatch()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var root = new DirectoryEntry();
        using var search = AdLdapClient.CreateIdentitySearcher(root, "(sAMAccountName=alice)", null);

        Assert.Equal(ReferralChasingOption.None, search.ReferralChasing);
        Assert.Equal(DereferenceAlias.Never, search.DerefAlias);
        Assert.Equal(2, search.SizeLimit);
    }

    [Theory]
    [InlineData(0, null, false)]
    [InlineData(2, @"EXAMPLE\alice", false)]
    [InlineData(1, @"OTHER\alice", false)]
    [InlineData(1, null, false)]
    [InlineData(1, @"example\Alice", true)]
    public async Task NovellProfile_RequiresOneVerifiedEntryEvenWithMaxResultsOneAsync(int count, string? canonical, bool expected)
    {
        var config = CreateConfig();
        config.MaxResults = 1;
        config.Attributes = ["mail"];
        var token = TestContext.Current.CancellationToken;
        var connection = new Mock<ILdapConnection>(MockBehavior.Strict);
        var results = new Mock<ILdapSearchResults>(MockBehavior.Strict);
        results.SetupSequence(r => r.HasMoreAsync(token)).ReturnsAsync(count > 0).ReturnsAsync(count > 1);
        var attributes = new LdapAttributeSet();
        if (canonical is not null)
        {
            attributes.Add(new LdapAttribute("msDS-PrincipalName", canonical));
        }
        results.Setup(r => r.NextAsync(token)).ReturnsAsync(new LdapEntry("CN=Alice,DC=example", attributes));
        connection.Setup(c => c.SearchAsync(config.SearchBase, LdapConnection.ScopeSub, "(sAMAccountName=alice)",
            It.Is<string[]>(a => a.Contains("mail") && a.Contains("msDS-PrincipalName")), false,
            It.Is<LdapSearchConstraints>(c => !c.ReferralFollowing && c.Dereference == LdapSearchConstraints.DerefNever && c.MaxResults == 2), token)).ReturnsAsync(results.Object);

        var profile = await new NovellLdapClient(config).ReadBoundProfileAsync(connection.Object, @"u:EXAMPLE\alice", config.SearchBase, token);

        Assert.Equal(expected, profile is not null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NovellCredentialsOnly_UsesBindWithoutProfileLookupAsync(bool accepted)
    {
        var config = CreateConfig();
        config.Security = true;
        var token = TestContext.Current.CancellationToken;
        var connection = new Mock<ILdapConnection>(MockBehavior.Strict);
        var bind = connection.Setup(c => c.BindAsync(@"example\alice", "password",
            It.Is<LdapConstraints>(constraints => !constraints.ReferralFollowing), token));
        if (accepted)
        {
            bind.Returns(Task.CompletedTask);
        }
        else
        {
            bind.ThrowsAsync(new LdapException("invalid", LdapException.InvalidCredentials, null));
        }

        var result = await new NovellLdapClient(config).AuthenticateAsync(connection.Object, "alice", "password", false, null, token);

        Assert.Equal(accepted, result.IsValid);
        Assert.Null(result.LdapUserInfo);
        connection.VerifyAll();
    }

    [Fact]
    public async Task NovellProfile_WhenWhoAmIIsUnsupported_DoesNotAcceptPasswordAloneAsync()
    {
        var token = TestContext.Current.CancellationToken;
        var connection = new Mock<ILdapConnection>(MockBehavior.Strict);
        connection.Setup(c => c.BindAsync(@"example\alice", "password", It.IsAny<LdapConstraints>(), token))
            .Returns(Task.CompletedTask);
        connection.Setup(c => c.ExtendedOperationAsync(It.IsAny<LdapWhoAmIOperation>(), It.IsAny<LdapConstraints>(), token))
            .ThrowsAsync(new LdapException("unsupported", LdapException.ProtocolError, null));
        var config = CreateConfig();
        config.Security = true;

        var result = await new NovellLdapClient(config).AuthenticateAsync(connection.Object, "alice", "password", true, null, token);

        Assert.False(result.IsValid);
        Assert.Null(result.LdapUserInfo);
        connection.VerifyAll();
    }

    [Fact]
    public void AdCredentialsOnly_RejectsEmptyPasswordBeforeDiscovery()
    {
        IActiveDirectoryCredentialValidator client = new AdLdapClient();

        Assert.Throws<ArgumentException>(() => client.ValidateCredentials("alice", string.Empty));
    }

    private static LdapConfig CreateConfig()
    {
        return new LdapConfig
        {
            Url = "ldap.example.com",
            Domain = "example",
            SearchBase = "DC=example,DC=com",
            SearchFilter = "(&(objectClass=person)(uid={0}))"
        };
    }
}
