using Linger.Ldap.Contracts;
using Linger.Ldap.Novell;
using Novell.Directory.Ldap;

namespace Linger.Ldap.UnitTests;

public class LdapEntryExtensionsTests
{
    [Fact]
    public void ToLdapUserInfo_PreservesMultiValueAttributes()
    {
        var attributes = new LdapAttributeSet();
        attributes.Add(new LdapAttribute(LdapUserType.ProxyAddresses, ["smtp:alice@example.com", "SMTP:alice@corp.example.com"]));
        attributes.Add(new LdapAttribute(LdapUserType.OtherTelephone, ["1001", "1002"]));
        var entry = new LdapEntry("CN=Alice,DC=example,DC=com", attributes);

        var user = entry.ToLdapUserInfo();

        Assert.NotNull(user);
        Assert.Equal(["smtp:alice@example.com", "SMTP:alice@corp.example.com"], user.ProxyAddresses);
        Assert.Equal(["1001", "1002"], user.OtherTelephone);
    }
}
