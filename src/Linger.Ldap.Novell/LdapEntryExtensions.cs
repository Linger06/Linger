using System.Globalization;
using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Novell.Directory.Ldap;

namespace Linger.Ldap.Novell;

public static class LdapEntryExtensions
{
    /// <summary>
    /// Converts an LdapEntry to an LdapUserInfo object
    /// </summary>
    /// <param name="user">The LdapEntry to convert</param>
    /// <returns>An LdapUserInfo object or null if the attribute set is null</returns>
    public static LdapUserInfo? ToLdapUserInfo(this LdapEntry user)
    {
        var attributeSet = user.GetAttributeSet();
        if (attributeSet is null) return null;

        var userInfo = new LdapUserInfo();

        // 基本标识信息
        MapIdentificationInfo(userInfo, attributeSet);

        // 个人信息
        MapPersonalInfo(userInfo, attributeSet);

        // 联系信息
        MapContactInfo(userInfo, attributeSet);

        // 组织信息
        MapOrganizationInfo(userInfo, attributeSet);

        // 地址信息
        MapAddressInfo(userInfo, attributeSet);

        // 系统信息
        MapSystemInfo(userInfo, attributeSet);

        // 安全信息
        MapSecurityInfo(userInfo, attributeSet);

        return userInfo;
    }

    private static void MapIdentificationInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.SamAccountName = GetAttributeValue(attributeSet, LdapUserType.SamAccountName);
        userInfo.DisplayName = GetAttributeValue(attributeSet, LdapUserType.DisplayName);
        userInfo.Upn = GetAttributeValue(attributeSet, LdapUserType.Upn);
        userInfo.Name = GetAttributeValue(attributeSet, LdapUserType.Name);
        userInfo.Dn = GetAttributeValue(attributeSet, LdapUserType.Dn);
    }

    private static void MapPersonalInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.FirstName = GetAttributeValue(attributeSet, LdapUserType.FirstName);
        userInfo.LastName = GetAttributeValue(attributeSet, LdapUserType.LastName);
        userInfo.Initials = GetAttributeValue(attributeSet, LdapUserType.Initials);
        userInfo.Description = GetAttributeValue(attributeSet, LdapUserType.Description);
    }

    private static void MapContactInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        // 电子邮件相关
        userInfo.Email = GetAttributeValue(attributeSet, LdapUserType.Email);
        userInfo.LyncAddress = GetAttributeValue(attributeSet, LdapUserType.LyncAddress);
        userInfo.ProxyAddresses = GetAttributeValues(attributeSet, LdapUserType.ProxyAddresses);
        userInfo.WebPage = GetAttributeValue(attributeSet, LdapUserType.WebPage);

        // 电话号码相关
        userInfo.TelephoneNumber = GetAttributeValue(attributeSet, LdapUserType.TelephoneNumber);
        userInfo.Mobile = GetAttributeValue(attributeSet, LdapUserType.Mobile);
        userInfo.HomePhone = GetAttributeValue(attributeSet, LdapUserType.HomePhone);
        userInfo.Pager = GetAttributeValue(attributeSet, LdapUserType.Pager);
        userInfo.Fax = GetAttributeValue(attributeSet, LdapUserType.Fax);
        userInfo.IpPhone = GetAttributeValue(attributeSet, LdapUserType.IpPhone);
        userInfo.OtherTelephone = GetAttributeValues(attributeSet, LdapUserType.OtherTelephone);
    }

    private static void MapOrganizationInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.Company = GetAttributeValue(attributeSet, LdapUserType.Company);
        userInfo.Department = GetAttributeValue(attributeSet, LdapUserType.Department);
        userInfo.Title = GetAttributeValue(attributeSet, LdapUserType.Title);
        userInfo.Manager = GetAttributeValue(attributeSet, LdapUserType.Manager);
        userInfo.EmployeeId = GetAttributeValue(attributeSet, LdapUserType.EmployeeId);
        userInfo.EmployeeNumber = GetAttributeValue(attributeSet, LdapUserType.EmployeeNumber);
        userInfo.Office = GetAttributeValue(attributeSet, LdapUserType.Office);
    }

    private static void MapAddressInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.Street = GetAttributeValue(attributeSet, LdapUserType.Street);
        userInfo.PostOfficeBox = GetAttributeValue(attributeSet, LdapUserType.PostOfficeBox);
        userInfo.City = GetAttributeValue(attributeSet, LdapUserType.City);
        userInfo.State = GetAttributeValue(attributeSet, LdapUserType.State);
        userInfo.PostalCode = GetAttributeValue(attributeSet, LdapUserType.PostalCode);
        userInfo.Country = GetAttributeValue(attributeSet, LdapUserType.Country);
    }

    private static void MapSystemInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.UserWorkstations = GetAttributeValue(attributeSet, LdapUserType.UserWorkstations);
        userInfo.ProfilePath = GetAttributeValue(attributeSet, LdapUserType.ProfilePath);
        userInfo.HomeDrive = GetAttributeValue(attributeSet, LdapUserType.HomeDrive);
        userInfo.HomeDirectory = GetAttributeValue(attributeSet, LdapUserType.HomeDirectory);
        userInfo.ExMailboxDb = GetAttributeValue(attributeSet, LdapUserType.ExMailboxDb);
        userInfo.ExtensionAttribute1 = GetAttributeValue(attributeSet, LdapUserType.ExtensionAttribute1);
        userInfo.UserType = attributeSet.ContainsKey(LdapUserType.ExMailboxDb) ? "UserMailbox" : "User";
        userInfo.MemberOf = GetMemberOf(attributeSet);

        if (attributeSet.ContainsKey(LdapUserType.WhenCreated))
        {
            userInfo.WhenCreated = ParseWhenCreated(attributeSet.GetAttribute(LdapUserType.WhenCreated));
        }
    }

    private static void MapSecurityInfo(LdapUserInfo userInfo, LdapAttributeSet attributeSet)
    {
        userInfo.Status = GetStatus(attributeSet);
        userInfo.PwdLastSet = GetPwdLastSet(attributeSet);
    }

    private static string? GetAttributeValue(LdapAttributeSet attributeSet, string attributeName)
    {
        return attributeSet.ContainsKey(attributeName) ? attributeSet.GetAttribute(attributeName).StringValue : null;
    }

    private static string[]? GetAttributeValues(LdapAttributeSet attributeSet, string attributeName)
    {
        return attributeSet.ContainsKey(attributeName)
            ? attributeSet.GetAttribute(attributeName).StringValueArray
            : null;
    }

    private static string[]? GetMemberOf(LdapAttributeSet attributeSet)
    {
        return GetAttributeValues(attributeSet, LdapUserType.MemberOf);
    }

    private static DateTime? ParseWhenCreated(LdapAttribute? attribute)
    {
        if (attribute is null) return null;

        try
        {
            return DateTime.ParseExact(attribute.StringValue,
                "yyyyMMddHHmmss.0Z",
                CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetPwdLastSet(LdapAttributeSet attributeSet)
    {
        if (!attributeSet.ContainsKey(LdapUserType.PwdLastSet)) return null;

        var attribute = attributeSet.GetAttribute(LdapUserType.PwdLastSet);
        if (attribute.StringValue == "0") return "Password has never been changed.";

        try
        {
            return DateTime.FromFileTime(attribute.StringValue.ToLongOrDefault())
                .ToString(CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetStatus(LdapAttributeSet attributeSet)
    {
        if (!attributeSet.ContainsKey(LdapUserType.UserAccountControl)) return null;

        try
        {
            var userAccountControl = attributeSet.GetAttribute(LdapUserType.UserAccountControl).StringValue;
            if (!userAccountControl.TryToInt(out var num))
            {
                return "Unknown";
            }

            return (num & 2) > 0 ? "Disabled" : "Enabled";
        }
        catch
        {
            return "Unknown";
        }
    }
}
