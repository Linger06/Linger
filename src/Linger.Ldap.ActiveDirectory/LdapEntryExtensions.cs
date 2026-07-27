using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using Linger.Extensions.Core;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
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
        if (userPrincipal is null) return null;

        if (userPrincipal.GetUnderlyingObject() is not DirectoryEntry directoryEntry) return null;

        var userInfo = CreateUserInfo(
            propertyName => GetPropertyValue(directoryEntry, propertyName),
            propertyName => GetPropertyValues(directoryEntry, propertyName));
        userInfo.SamAccountName = userPrincipal.SamAccountName;
        userInfo.DisplayName = userPrincipal.DisplayName;
        userInfo.Upn = userPrincipal.UserPrincipalName;
        userInfo.Name = userPrincipal.Name;
        userInfo.Dn = userPrincipal.DistinguishedName;
        userInfo.FirstName = userPrincipal.GivenName;
        userInfo.LastName = userPrincipal.Surname;
        userInfo.Description = userPrincipal.Description;

        // 安全信息使用 UserPrincipal 特有的方法
        MapSpecialUserPrincipalProperties(userInfo, userPrincipal);

        return userInfo;
    }

    private static void MapSpecialUserPrincipalProperties(AdUserInfo userInfo, UserPrincipal user)
    {
        // 处理只能从 UserPrincipal 获取的属性
        userInfo.Status = GetUserStatus(user);
        userInfo.PwdLastSet = user.LastPasswordSet?.ToString(CultureInfo.InvariantCulture);
        userInfo.PwdExpirationLeftDays = GetPasswordExpirationDays(user);
        userInfo.AccountExpires = user.AccountExpirationDate?.ToString(CultureInfo.InvariantCulture);
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
            using var de = user.Context.ConnectedServer is not null
                ? new DirectoryEntry($"LDAP://{user.Context.ConnectedServer}")
                : new DirectoryEntry();

            // 获取最大密码期限（以 100 纳秒为单位的负值）
            var maxPwdAge = (long?)de.Properties["maxPwdAge"].Value;
            if (!maxPwdAge.HasValue || maxPwdAge.Value == 0) return null;

            // 转换为天数（去掉负号并转换为天数）
            // 使用decimal确保精确计算
            const decimal TicksPerDay = 864000000000M; // 24 * 60 * 60 * 10000000 (一天的 100 纳秒数)
            var maxPwdAgeDays = Math.Abs(maxPwdAge.Value) / TicksPerDay;

            // 计算剩余天数
            var expirationDate = lastSet.Value.AddDays((double)maxPwdAgeDays);
            var daysLeft = (expirationDate - DateTime.Now).Days;

            return daysLeft.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            // 如果无法获取密码过期信息，返回null
            return null;
        }
    }

    /// <summary>
    /// Converts Active Directory search results to user information without issuing additional directory queries.
    /// </summary>
    /// <param name="resultCollection">The search results to convert.</param>
    /// <returns>The converted user information.</returns>
    public static List<AdUserInfo> ToAdUsersInfo(this SearchResultCollection resultCollection)
    {
        var userList = new List<AdUserInfo>();

        foreach (SearchResult result in resultCollection)
        {
            var userInfo = CreateUserInfo(
                propertyName => GetPropertyValue(result, propertyName),
                propertyName => GetPropertyValues(result, propertyName));
            MapSearchResultSecurityInfo(userInfo, result);
            userList.Add(userInfo);
        }

        return userList;
    }

    private static string? GetPropertyValue(SearchResult result, string propertyName)
    {
        var values = result.Properties[propertyName];

        return values.Count > 0 ? values[0]?.ToString() : null;
    }

    private static string[]? GetPropertyValues(SearchResult result, string propertyName)
    {
        var values = result.Properties[propertyName];
        if (values.Count == 0)
        {
            return null;
        }

        return values
            .Cast<object>()
            .Select(value => value.ToString())
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
    }

    private static void MapSearchResultSecurityInfo(AdUserInfo userInfo, SearchResult result)
    {
        var userAccountControlValue = GetPropertyValue(result, LdapUserType.UserAccountControl);
        if (!int.TryParse(userAccountControlValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userAccountControl))
        {
            SetDefaultSecurityInfo(userInfo);
            return;
        }

        var isDisabled = IsAccountDisabled(userAccountControl);
        var accountExpires = GetAccountExpirationDate(GetPropertyValue(result, LdapUserType.AccountExpires) ?? string.Empty);
        var isExpired = accountExpires?.Date <= DateTime.Now.Date;
        userInfo.Status = GetAccountStatus(isDisabled, isLocked: false, isExpired);
        userInfo.AccountExpires = accountExpires?.ToString(CultureInfo.InvariantCulture);

        var pwdLastSet = GetPropertyValue(result, LdapUserType.PwdLastSet);
        if (!long.TryParse(pwdLastSet, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lastSetValue))
        {
            userInfo.PwdLastSet = pwdLastSet is null ? PasswordStatus.Unknown : PasswordStatus.InvalidFormat;
            userInfo.PwdExpirationLeftDays = PasswordStatus.Unknown;
            return;
        }

        if (lastSetValue == 0)
        {
            userInfo.PwdLastSet = PasswordStatus.NeverChanged;
        }
        else
        {
            try
            {
                userInfo.PwdLastSet = DateTime.FromFileTime(lastSetValue).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException)
            {
                userInfo.PwdLastSet = PasswordStatus.InvalidFormat;
            }
        }

        userInfo.PwdExpirationLeftDays = (userAccountControl & UserAccountControl.PasswordNeverExpires) != 0
            ? PasswordStatus.NeverExpires
            : PasswordStatus.UnableToCalculate;
    }

    /// <summary>
    /// Converts an Active Directory entry to user information.
    /// </summary>
    /// <param name="entry">The directory entry to convert.</param>
    /// <returns>The converted user information.</returns>
    public static AdUserInfo ToAdUserInfo(this DirectoryEntry entry)
    {
        var userInfo = CreateUserInfo(
            propertyName => GetPropertyValue(entry, propertyName),
            propertyName => GetPropertyValues(entry, propertyName));
        MapSecurityInfo(userInfo, entry);

        return userInfo;
    }

    private static AdUserInfo CreateUserInfo(
        Func<string, string?> getValue,
        Func<string, string[]?> getValues)
    {
        var exMailboxDb = getValue(LdapUserType.ExMailboxDb);
        var userInfo = new AdUserInfo
        {
            SamAccountName = getValue(LdapUserType.SamAccountName),
            DisplayName = getValue(LdapUserType.DisplayName),
            Upn = getValue(LdapUserType.Upn),
            Name = getValue(LdapUserType.Name),
            Dn = getValue(LdapUserType.Dn),
            FirstName = getValue(LdapUserType.FirstName),
            LastName = getValue(LdapUserType.LastName),
            Description = getValue(LdapUserType.Description),
            Initials = getValue(LdapUserType.Initials),
            Email = getValue(LdapUserType.Email),
            LyncAddress = getValue(LdapUserType.LyncAddress),
            ProxyAddresses = JoinValues(getValues(LdapUserType.ProxyAddresses), " ^ "),
            WebPage = getValue(LdapUserType.WebPage),
            TelephoneNumber = getValue(LdapUserType.TelephoneNumber),
            Mobile = getValue(LdapUserType.Mobile),
            HomePhone = getValue(LdapUserType.HomePhone),
            Pager = getValue(LdapUserType.Pager),
            Fax = getValue(LdapUserType.Fax),
            IpPhone = getValue(LdapUserType.IpPhone),
            OtherTelephone = JoinValues(getValues(LdapUserType.OtherTelephone), "^"),
            Company = getValue(LdapUserType.Company),
            Department = getValue(LdapUserType.Department),
            Title = getValue(LdapUserType.Title),
            Manager = getValue(LdapUserType.Manager),
            EmployeeId = getValue(LdapUserType.EmployeeId),
            EmployeeNumber = getValue(LdapUserType.EmployeeNumber),
            Office = getValue(LdapUserType.Office),
            Street = getValue(LdapUserType.Street),
            PostOfficeBox = getValue(LdapUserType.PostOfficeBox),
            City = getValue(LdapUserType.City),
            State = getValue(LdapUserType.State),
            PostalCode = getValue(LdapUserType.PostalCode),
            Country = getValue(LdapUserType.Country),
            UserWorkstations = getValue(LdapUserType.UserWorkstations),
            ProfilePath = getValue(LdapUserType.ProfilePath),
            HomeDrive = getValue(LdapUserType.HomeDrive),
            HomeDirectory = getValue(LdapUserType.HomeDirectory),
            ExMailboxDb = exMailboxDb,
            ExtensionAttribute1 = getValue(LdapUserType.ExtensionAttribute1),
            UserType = exMailboxDb is not null ? "UserMailbox" : "User",
            MemberOf = getValues(LdapUserType.MemberOf)
        };

        var createdDate = getValue(LdapUserType.WhenCreated);
        if (DateTime.TryParse(createdDate, out var whenCreated))
        {
            userInfo.WhenCreated = whenCreated.ToLocalTime();
        }

        return userInfo;
    }

    private static string? JoinValues(string[]? values, string separator) =>
        values is { Length: > 0 } ? string.Join(separator, values) : null;

    private static string[]? GetPropertyValues(DirectoryEntry entry, string propertyName)
    {
        try
        {
            var values = entry.Properties[propertyName];
            return values.Count > 0 ? values.Cast<object>().Select(value => value.ToString()!).ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    private static void MapSecurityInfo(AdUserInfo userInfo, DirectoryEntry entry)
    {
        try
        {
            var userAccountControlStr = GetPropertyValue(entry, LdapUserType.UserAccountControl);
            if (userAccountControlStr is null || !int.TryParse(userAccountControlStr, out var userAccountControl))
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
        (userAccountControl & UserAccountControl.Disabled) != 0;

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
            return GetAccountExpirationDate(expiresValue)?.ToString(CultureInfo.InvariantCulture);
        }
        return null;
    }

    private static void GetPasswordInfo(AdUserInfo userInfo, DirectoryEntry entry, int userAccountControl)
    {
        var pwdLastSet = GetPropertyValue(entry, LdapUserType.PwdLastSet);

        if (pwdLastSet is null)
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
            : DateTime.FromFileTime(lastSetValue).ToString(CultureInfo.InvariantCulture);

        userInfo.PwdExpirationLeftDays =
            (userAccountControl & UserAccountControl.PasswordNeverExpires) != 0
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
