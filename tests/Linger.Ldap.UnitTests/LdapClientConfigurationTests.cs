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
