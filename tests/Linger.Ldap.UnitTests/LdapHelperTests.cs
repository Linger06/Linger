using Linger.Ldap.Contracts;

namespace Linger.Ldap.UnitTests;

public class LdapHelperTests
{
    [Fact]
    public void BuildUserSearchFilter_WhenConfiguredTemplateHasNoPlaceholder_UsesFallback()
    {
        var filter = LdapHelper.BuildUserSearchFilter(
            "alice",
            exactMatch: true,
            "(&(objectClass=user))",
            "(&(objectClass=person)(uid={0}))",
            out var usedFallback);

        Assert.True(usedFallback);
        Assert.Equal("(&(objectClass=person)(uid=alice))", filter);
    }

    [Fact]
    public void BuildUserSearchFilter_EscapesExactMatchValue()
    {
        var filter = LdapHelper.BuildUserSearchFilter(
            "a*(b)\\c",
            exactMatch: true,
            configuredTemplate: null,
            "(uid={0})",
            out var usedFallback);

        Assert.False(usedFallback);
        Assert.Equal("(uid=a\\2a\\28b\\29\\5cc)", filter);
    }
}
