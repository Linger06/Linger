using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Linger.Extensions.Core;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using Linger.Ldap.ActiveDirectory.Constants;
using Linger.Ldap.Contracts;
using static Linger.Ldap.ActiveDirectory.Constants.ActiveDirectoryConstants;

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

        if (userPrincipal.GetUnderlyingObject() is not DirectoryEntry directoryEntry) return null;

        var userInfo = new AdUserInfo
        {
            // 基本标识信息 - 优先使用 UserPrincipal 中的值
            SamAccountName = userPrincipal.SamAccountName,
            DisplayName = userPrincipal.DisplayName,
            Upn = userPrincipal.UserPrincipalName,
            Name = userPrincipal.Name,
            Dn = userPrincipal.DistinguishedName,

            // 个人信息 - 优先使用 UserPrincipal 中的值
            FirstName = userPrincipal.GivenName,
            LastName = userPrincipal.Surname,
            Description = userPrincipal.Description
        };

        // 使用 DirectoryEntry 填充其余属性
        MapContactInfo(userInfo, directoryEntry);
        MapOrganizationInfo(userInfo, directoryEntry);
        MapAddressInfo(userInfo, directoryEntry);
        MapSystemInfo(userInfo, directoryEntry);

        // 安全信息使用 UserPrincipal 特有的方法
        MapSpecialUserPrincipalProperties(userInfo, userPrincipal);

        return userInfo;
    }

    private static void MapSpecialUserPrincipalProperties(AdUserInfo userInfo, UserPrincipal user)
    {
        // 处理只能从 UserPrincipal 获取的属性
        userInfo.Status = GetUserStatus(user);
        userInfo.PwdLastSet = user.LastPasswordSet?.ToString(ExtensionMethodSetting.DefaultCulture);
        userInfo.PwdExpirationLeftDays = GetPasswordExpirationDays(user);
        userInfo.AccountExpires = user.AccountExpirationDate?.ToString(ExtensionMethodSetting.DefaultCulture);
    }

    private static string? GetPropertyValue(DirectoryEntry entry, string propertyName)
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

    private static string? GetMultiValueProperty(DirectoryEntry entry, string propertyName, string separator)
    {
        try
        {
            var prop = entry.Properties[propertyName];
            if (prop.Count == 0) return null;

            var values = new List<string>();
            foreach (var value in prop)
            {
                if (value?.ToString() is { } strValue)
                {
                    values.Add(strValue);
                }
            }

            return values.Count > 0 ? string.Join(separator, values) : null;
        }
        catch
        {
            return null;
        }
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
            if (user.IsAccountLockedOut()) return AccountStatus.Locked;
            if (!user.Enabled.GetValueOrDefault(true)) return AccountStatus.Disabled;
            if (user.AccountExpirationDate <= DateTime.Now) return AccountStatus.Expired;
            return AccountStatus.Enabled;
        }
        catch
        {
            return AccountStatus.Unknown;
        }
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
            const decimal TicksPerDay = 864000000000M; // 24 * 60 * 60 * 10000000 (一天的100纳秒数)
            var maxPwdAgeDays = Math.Abs(maxPwdAge.Value) / TicksPerDay;

            // 计算剩余天数
            var expirationDate = lastSet.Value.AddDays((double)maxPwdAgeDays);
            var daysLeft = (expirationDate - DateTime.Now).Days;

            return daysLeft.ToString(ExtensionMethodSetting.DefaultCulture);
        }
        catch (Exception)
        {
            // 如果无法获取密码过期信息，返回null
            return null;
        }
    }

    public static List<AdUserInfo> ToAdUsersInfo(this SearchResultCollection resultCollection)
    {
        var userList = new List<AdUserInfo>();

        foreach (SearchResult result in resultCollection)
        {
            using var entry = new DirectoryEntry(result.Path);
            var userInfo = entry.ToAdUserInfo();
            userList.Add(userInfo);
        }

        return userList;
    }

    public static AdUserInfo ToAdUserInfo(this DirectoryEntry entry)
    {
        var userInfo = new AdUserInfo();

        // 基本标识信息
        MapIdentificationInfo(userInfo, entry);

        // 个人信息
        MapPersonalInfo(userInfo, entry);

        // 联系信息
        MapContactInfo(userInfo, entry);

        // 组织信息 
        MapOrganizationInfo(userInfo, entry);

        // 地址信息
        MapAddressInfo(userInfo, entry);

        // 系统信息
        MapSystemInfo(userInfo, entry);

        // 安全信息
        MapSecurityInfo(userInfo, entry);

        return userInfo;
    }

    private static void MapIdentificationInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        userInfo.SamAccountName = GetPropertyValue(entry, LdapUserType.SamAccountName);
        userInfo.DisplayName = GetPropertyValue(entry, LdapUserType.DisplayName);
        userInfo.Upn = GetPropertyValue(entry, LdapUserType.Upn);
        userInfo.Name = GetPropertyValue(entry, LdapUserType.Name);
        userInfo.Dn = GetPropertyValue(entry, LdapUserType.Dn);
    }

    private static void MapPersonalInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        userInfo.FirstName = GetPropertyValue(entry, LdapUserType.FirstName);
        userInfo.LastName = GetPropertyValue(entry, LdapUserType.LastName);
        userInfo.Description = GetPropertyValue(entry, LdapUserType.Description);
        userInfo.Initials = GetPropertyValue(entry, LdapUserType.Initials);
    }

    private static void MapContactInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        // 电子邮件相关
        userInfo.Email = GetPropertyValue(entry, LdapUserType.Email);
        userInfo.LyncAddress = GetPropertyValue(entry, LdapUserType.LyncAddress);
        userInfo.ProxyAddresses = GetMultiValueProperty(entry, LdapUserType.ProxyAddresses, " ^ ");
        userInfo.WebPage = GetPropertyValue(entry, LdapUserType.WebPage);

        // 电话号码
        userInfo.TelephoneNumber = GetPropertyValue(entry, LdapUserType.TelephoneNumber);
        userInfo.Mobile = GetPropertyValue(entry, LdapUserType.Mobile);
        userInfo.HomePhone = GetPropertyValue(entry, LdapUserType.HomePhone);
        userInfo.Pager = GetPropertyValue(entry, LdapUserType.Pager);
        userInfo.Fax = GetPropertyValue(entry, LdapUserType.Fax);
        userInfo.IpPhone = GetPropertyValue(entry, LdapUserType.IpPhone);
        userInfo.OtherTelephone = GetMultiValueProperty(entry, LdapUserType.OtherTelephone, "^");
    }

    private static void MapOrganizationInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        userInfo.Company = GetPropertyValue(entry, LdapUserType.Company);
        userInfo.Department = GetPropertyValue(entry, LdapUserType.Department);
        userInfo.Title = GetPropertyValue(entry, LdapUserType.Title);
        userInfo.Manager = GetPropertyValue(entry, LdapUserType.Manager);
        userInfo.EmployeeId = GetPropertyValue(entry, LdapUserType.EmployeeId);
        userInfo.EmployeeNumber = GetPropertyValue(entry, LdapUserType.EmployeeNumber);
        userInfo.Office = GetPropertyValue(entry, LdapUserType.Office);
    }

    private static void MapAddressInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        userInfo.Street = GetPropertyValue(entry, LdapUserType.Street);
        userInfo.PostOfficeBox = GetPropertyValue(entry, LdapUserType.PostOfficeBox);
        userInfo.City = GetPropertyValue(entry, LdapUserType.City);
        userInfo.State = GetPropertyValue(entry, LdapUserType.State);
        userInfo.PostalCode = GetPropertyValue(entry, LdapUserType.PostalCode);
        userInfo.Country = GetPropertyValue(entry, LdapUserType.Country);
    }

    private static void MapSystemInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        userInfo.UserWorkstations = GetPropertyValue(entry, LdapUserType.UserWorkstations);
        userInfo.ProfilePath = GetPropertyValue(entry, LdapUserType.ProfilePath);
        userInfo.HomeDrive = GetPropertyValue(entry, LdapUserType.HomeDrive);
        userInfo.HomeDirectory = GetPropertyValue(entry, LdapUserType.HomeDirectory);
        userInfo.ExMailboxDb = GetPropertyValue(entry, LdapUserType.ExMailboxDb);
        userInfo.ExtensionAttribute1 = GetPropertyValue(entry, LdapUserType.ExtensionAttribute1);
        userInfo.UserType = GetPropertyValue(entry, LdapUserType.ExMailboxDb) != null ? "UserMailbox" : "User";
        userInfo.MemberOf = GetMemberOf(entry);    // 添加 MemberOf 属性映射

        var createdDate = GetPropertyValue(entry, LdapUserType.WhenCreated);
        if (createdDate != null && DateTime.TryParse(createdDate, out var whenCreated))
        {
            userInfo.WhenCreated = whenCreated.ToLocalTime();
        }
    }

    private static void MapSecurityInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        try
        {
            var userAccountControlStr = GetPropertyValue(entry, LdapUserType.UserAccountControl);
            if (userAccountControlStr == null || !int.TryParse(userAccountControlStr, out var userAccountControl))
            {
                SetDefaultSecurityInfo(userInfo);
                return;
            }

            // 获取账户状态
            var isDisabled = IsAccountDisabled(userAccountControl);
            var isLocked = IsAccountLocked(entry);
            var isExpired = IsAccountExpired(entry);
            userInfo.Status = GetAccountStatus(isDisabled, isLocked, isExpired);

            // 获取账户过期时间
            userInfo.AccountExpires = GetAccountExpiresDate(entry);

            // 获取密码相关信息
            GetPasswordInfo(userInfo, entry, userAccountControl);
        }
        catch
        {
            SetDefaultSecurityInfo(userInfo);
        }
    }

    private static bool IsAccountDisabled(int userAccountControl) =>
        (userAccountControl & ActiveDirectoryConstants.UserAccountControl.Disabled) != 0;

    private static bool IsAccountLocked(DirectoryEntry entry)
    {
        try
        {
            var isAccountLocked = entry.InvokeGet("IsAccountLocked");
            return isAccountLocked.ToBoolOrDefault();// Convert.ToBoolean(entry.InvokeGet("IsAccountLocked"));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAccountExpired(DirectoryEntry entry)
    {
        var expiresStr = GetPropertyValue(entry, LdapUserType.AccountExpires);
        if (long.TryParse(expiresStr, out var expiresValue))
        {
            var expiresDate = GetAccountExpirationDate(expiresValue);
            return expiresDate?.Date <= DateTime.Now.Date;
        }
        return false;
    }

    private static string GetAccountStatus(bool isDisabled, bool isLocked, bool isExpired)
    {
        var status = new List<string>();
        if (isDisabled) status.Add("Disabled");
        if (isLocked) status.Add("Locked");
        if (isExpired) status.Add("Expired");
        return status.Count > 0 ? string.Join("&", status) : "Enabled";
    }

    private static string? GetAccountExpiresDate(DirectoryEntry entry)
    {
        var expiresStr = GetPropertyValue(entry, LdapUserType.AccountExpires);
        if (long.TryParse(expiresStr, out var expiresValue))
        {
            return GetAccountExpirationDate(expiresValue)?.ToString(ExtensionMethodSetting.DefaultCulture);
        }
        return null;
    }

    private static void GetPasswordInfo(AdUserInfo userInfo, DirectoryEntry entry, int userAccountControl)
    {
        var pwdLastSet = GetPropertyValue(entry, LdapUserType.PwdLastSet);

        if (pwdLastSet == null)
        {
            userInfo.PwdLastSet = PasswordStatus.Unknown;
            userInfo.PwdExpirationLeftDays = PasswordStatus.Unknown;
            return;
        }

        if (!long.TryParse(pwdLastSet, out var lastSetValue))
        {
            userInfo.PwdLastSet = PasswordStatus.InvalidFormat;
            userInfo.PwdExpirationLeftDays = PasswordStatus.Unknown;
            return;
        }

        userInfo.PwdLastSet = lastSetValue == 0
            ? PasswordStatus.NeverChanged
            : DateTime.FromFileTime(lastSetValue).ToString(ExtensionMethodSetting.DefaultCulture);

        userInfo.PwdExpirationLeftDays =
            (userAccountControl & ActiveDirectoryConstants.UserAccountControl.PasswordNeverExpires) != 0
                ? PasswordStatus.NeverExpires
                : GetPasswordExpirationInfo(entry, lastSetValue);
    }

    private static string GetPasswordExpirationInfo(DirectoryEntry entry, long lastSetValue)
    {
        if (lastSetValue == 0) return PasswordStatus.NeverChanged;

        try
        {
            using var de = entry.Parent;
            var maxPwdAge = (long?)de.Properties["maxPwdAge"].Value;

            if (!maxPwdAge.HasValue) return PasswordStatus.NoExpirationPolicy;
            if (maxPwdAge.Value == 0) return PasswordStatus.NoExpirationSet;
            if (maxPwdAge.Value == TimeConstants.NeverExpiresFlag) return PasswordStatus.DomainPolicyNeverExpires;

            var maxPwdDays = Math.Abs(maxPwdAge.Value) / TimeConstants.TicksPerDay;
            var expirationDate = DateTime.FromFileTime(lastSetValue).AddDays((double)maxPwdDays);
            var daysLeft = (expirationDate - DateTime.Now).Days;

            return daysLeft >= 0
                ? $"Expires in {daysLeft} days"
                : $"Expired {Math.Abs(daysLeft)} days ago";
        }
        catch
        {
            return PasswordStatus.UnableToCalculate;
        }
    }

    private static void SetDefaultSecurityInfo(AdUserInfo userInfo)
    {
        userInfo.Status = "Unknown";
        userInfo.PwdLastSet = "Unknown";
        userInfo.PwdExpirationLeftDays = "Unknown";
        userInfo.AccountExpires = null;
    }

    private static DateTime? GetAccountExpirationDate(object accountExpiresValue)
    {
        try
        {
            var accountExpiresLong = accountExpiresValue.ToLongOrDefault();
            if (accountExpiresLong == 0 || accountExpiresLong == TimeConstants.NoExpiryDate)
            {
                return null;
            }
            return DateTime.FromFileTime(accountExpiresLong);
        }
        catch
        {
            return null;
        }
    }
}
