using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using Linger.Ldap.Contracts;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static class LdapEntryExtensions
{
    /// <summary>
    /// Converts a UserPrincipal to an AdUserInfo object
    /// </summary>
    /// <param name="userPrincipal">The UserPrincipal to convert</param>
    /// <returns>An AdUserInfo object or null if input is null</returns>
    public static AdUserInfo? ToAdUser(this UserPrincipal userPrincipal)
    {
        if (userPrincipal == null) return null;

        var directoryEntry = userPrincipal.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;
        if (directoryEntry == null) return null;

        var userInfo = new AdUserInfo();

        // 基本标识信息
        MapIdentificationInfo(userInfo, userPrincipal);

        // 个人信息
        MapPersonalInfo(userInfo, userPrincipal, directoryEntry);

        // 联系信息
        MapContactInfo(userInfo, userPrincipal, directoryEntry);

        // 组织信息
        MapOrganizationInfo(userInfo, directoryEntry);

        // 地址信息
        MapAddressInfo(userInfo, directoryEntry);

        // 系统和账户信息
        MapSystemInfo(userInfo, userPrincipal, directoryEntry);

        // 安全信息
        MapSecurityInfo(userInfo, userPrincipal);

        return userInfo;
    }

    private static void MapIdentificationInfo(AdUserInfo userInfo, UserPrincipal user)
    {
        userInfo.SamAccountName = user.SamAccountName;
        userInfo.DisplayName = user.DisplayName;
        userInfo.Upn = user.UserPrincipalName;
        userInfo.Name = user.Name;
        userInfo.Dn = user.DistinguishedName;
    }

    private static void MapPersonalInfo(AdUserInfo userInfo, UserPrincipal user, System.DirectoryServices.DirectoryEntry entry)
    {
        userInfo.FirstName = user.GivenName;
        userInfo.LastName = user.Surname;
        userInfo.Description = user.Description;
        userInfo.Initials = GetPropertyValue(entry, LdapUserType.Initials);
    }

    private static void MapContactInfo(AdUserInfo userInfo, UserPrincipal user, System.DirectoryServices.DirectoryEntry entry)
    {
        // 电子邮件
        userInfo.Email = user.EmailAddress;
        userInfo.LyncAddress = GetPropertyValue(entry, LdapUserType.LyncAddress);
        userInfo.ProxyAddresses = GetPropertyValue(entry, LdapUserType.ProxyAddresses);
        userInfo.WebPage = GetPropertyValue(entry, LdapUserType.WebPage);

        // 电话号码
        userInfo.TelephoneNumber = user.VoiceTelephoneNumber;
        userInfo.Mobile = GetPropertyValue(entry, LdapUserType.Mobile);
        userInfo.HomePhone = GetPropertyValue(entry, LdapUserType.HomePhone);
        userInfo.Pager = GetPropertyValue(entry, LdapUserType.Pager);
        userInfo.Fax = GetPropertyValue(entry, LdapUserType.Fax);
        userInfo.IpPhone = GetPropertyValue(entry, LdapUserType.IpPhone);
    }

    private static void MapOrganizationInfo(AdUserInfo userInfo, System.DirectoryServices.DirectoryEntry entry)
    {
        userInfo.Company = GetPropertyValue(entry, LdapUserType.Company);
        userInfo.Department = GetPropertyValue(entry, LdapUserType.Department);
        userInfo.Title = GetPropertyValue(entry, LdapUserType.Title);
        userInfo.Manager = GetPropertyValue(entry, LdapUserType.Manager);
        userInfo.EmployeeId = GetPropertyValue(entry, LdapUserType.EmployeeId);
        userInfo.EmployeeNumber = GetPropertyValue(entry, LdapUserType.EmployeeNumber);
        userInfo.Office = GetPropertyValue(entry, LdapUserType.Office);
    }

    private static void MapAddressInfo(AdUserInfo userInfo, System.DirectoryServices.DirectoryEntry entry)
    {
        userInfo.Street = GetPropertyValue(entry, LdapUserType.Street);
        userInfo.PostOfficeBox = GetPropertyValue(entry, LdapUserType.PostOfficeBox);
        userInfo.City = GetPropertyValue(entry, LdapUserType.City);
        userInfo.State = GetPropertyValue(entry, LdapUserType.State);
        userInfo.PostalCode = GetPropertyValue(entry, LdapUserType.PostalCode);
        userInfo.Country = GetPropertyValue(entry, LdapUserType.Country);
    }

    private static void MapSystemInfo(AdUserInfo userInfo, UserPrincipal user, System.DirectoryServices.DirectoryEntry entry)
    {
        userInfo.WhenCreated = GetWhenCreated(entry);
        userInfo.UserType = GetUserType(entry);
        userInfo.MemberOf = GetMemberOf(entry);
        userInfo.UserWorkstations = GetPropertyValue(entry, LdapUserType.UserWorkstations);
        userInfo.ProfilePath = GetPropertyValue(entry, LdapUserType.ProfilePath);
        userInfo.HomeDrive = GetPropertyValue(entry, LdapUserType.HomeDrive);
        userInfo.HomeDirectory = GetPropertyValue(entry, LdapUserType.HomeDirectory);
        userInfo.ExMailboxDb = GetPropertyValue(entry, LdapUserType.ExMailboxDb);
        userInfo.ExtensionAttribute1 = GetPropertyValue(entry, LdapUserType.ExtensionAttribute1);
    }

    private static void MapSecurityInfo(AdUserInfo userInfo, UserPrincipal user)
    {
        userInfo.Status = GetUserStatus(user);
        userInfo.PwdLastSet = GetPwdLastSet(user);
        userInfo.PwdExpirationLeftDays = GetPasswordExpirationDays(user);
        userInfo.AccountExpires = user.AccountExpirationDate?.ToString();
    }

    private static string? GetPropertyValue(System.DirectoryServices.DirectoryEntry entry, string propertyName)
    {
        try
        {
            return entry.Properties[propertyName].Value?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? GetWhenCreated(System.DirectoryServices.DirectoryEntry entry)
    {
        var whenCreated = GetPropertyValue(entry, LdapUserType.WhenCreated);
        return DateTime.TryParse(whenCreated, out var result) ? result : null;
    }

    private static string? GetUserType(System.DirectoryServices.DirectoryEntry entry)
    {
        return GetPropertyValue(entry, LdapUserType.ExMailboxDb) != null ? "UserMailbox" : "User";
    }

    private static string[]? GetMemberOf(System.DirectoryServices.DirectoryEntry entry)
    {
        try
        {
            var memberOf = entry.Properties[LdapUserType.MemberOf];
            return memberOf.Count > 0 ? memberOf.Cast<string>().ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GetUserStatus(UserPrincipal user)
    {
        try
        {
            if (user.IsAccountLockedOut()) return "Locked";
            if (!user.Enabled.GetValueOrDefault(true)) return "Disabled";
            if (user.AccountExpirationDate <= DateTime.Now) return "Expired";
            return "Enabled";
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string? GetPwdLastSet(UserPrincipal user)
    {
        return user.LastPasswordSet?.ToString();
    }

    private static string? GetPasswordExpirationDays(UserPrincipal user)
    {
        try
        {
            if (user.PasswordNeverExpires) return null;

            var lastSet = user.LastPasswordSet;
            if (!lastSet.HasValue) return null;

            // 获取域控制器的 DirectoryEntry
            using var de = user.Context.ConnectedServer != null
                ? new System.DirectoryServices.DirectoryEntry($"LDAP://{user.Context.ConnectedServer}")
                : new System.DirectoryServices.DirectoryEntry();

            // 获取最大密码期限（以100纳秒为单位的负值）
            var maxPwdAge = (long?)de.Properties["maxPwdAge"].Value;
            if (!maxPwdAge.HasValue || maxPwdAge.Value == 0) return null;

            // 转换为天数（去掉负号并转换为天数）
            // 使用decimal确保精确计算
            const decimal ticksPerDay = 864000000000M; // 24 * 60 * 60 * 10000000 (一天的100纳秒数)
            var maxPwdAgeDays = Math.Abs(maxPwdAge.Value) / ticksPerDay;

            // 计算剩余天数
            var expirationDate = lastSet.Value.AddDays((double)maxPwdAgeDays);
            var daysLeft = (expirationDate - DateTime.Now).Days;

            return daysLeft.ToString();
        }
        catch (Exception)
        {
            // 如果无法获取密码过期信息，返回null
            return null;
        }
    }
}