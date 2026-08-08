using System.DirectoryServices;
using System.Globalization;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using Linger.Ldap.Contracts;
using static Linger.Ldap.ActiveDirectory.Constants.ActiveDirectoryConstants;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal static class LdapEntryExtensions
{
    internal static List<LdapUserInfo> ToLdapUsersInfo(this SearchResultCollection resultCollection)
    {
        var userList = new List<LdapUserInfo>();

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

    private static void MapSearchResultSecurityInfo(LdapUserInfo userInfo, SearchResult result)
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
        var lastSetValue = SetPwdLastSet(userInfo, pwdLastSet);
        if (lastSetValue is null)
        {
            userInfo.PwdExpirationLeftDays = PasswordStatus.Unknown;
            return;
        }

        userInfo.PwdExpirationLeftDays = (userAccountControl & UserAccountControl.PasswordNeverExpires) != 0
            ? PasswordStatus.NeverExpires
            : PasswordStatus.UnableToCalculate;
    }

    /// <summary>
    /// Parses the raw pwdLastSet value into <see cref="LdapUserInfo.PwdLastSet"/>.
    /// Returns the parsed file-time value, or null when it could not be interpreted.
    /// </summary>
    private static long? SetPwdLastSet(LdapUserInfo userInfo, string? pwdLastSet)
    {
        if (pwdLastSet is null)
        {
            userInfo.PwdLastSet = PasswordStatus.Unknown;
            return null;
        }

        if (!long.TryParse(pwdLastSet, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lastSetValue))
        {
            userInfo.PwdLastSet = PasswordStatus.InvalidFormat;
            return null;
        }

        if (lastSetValue == 0)
        {
            userInfo.PwdLastSet = PasswordStatus.NeverChanged;
            return lastSetValue;
        }

        try
        {
            userInfo.PwdLastSet = DateTime.FromFileTime(lastSetValue).ToString(CultureInfo.InvariantCulture);
            return lastSetValue;
        }
        catch (ArgumentOutOfRangeException)
        {
            userInfo.PwdLastSet = PasswordStatus.InvalidFormat;
            return null;
        }
    }

    private static LdapUserInfo CreateUserInfo(
        Func<string, string?> getValue,
        Func<string, string[]?> getValues)
    {
        var exMailboxDb = getValue(LdapUserType.ExMailboxDb);
        var userInfo = new LdapUserInfo
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
            ProxyAddresses = getValues(LdapUserType.ProxyAddresses),
            WebPage = getValue(LdapUserType.WebPage),
            TelephoneNumber = getValue(LdapUserType.TelephoneNumber),
            Mobile = getValue(LdapUserType.Mobile),
            HomePhone = getValue(LdapUserType.HomePhone),
            Pager = getValue(LdapUserType.Pager),
            Fax = getValue(LdapUserType.Fax),
            IpPhone = getValue(LdapUserType.IpPhone),
            OtherTelephone = getValues(LdapUserType.OtherTelephone),
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

    private static bool IsAccountDisabled(int userAccountControl) =>
        (userAccountControl & UserAccountControl.Disabled) != 0;

    private static string GetAccountStatus(bool isDisabled, bool isLocked, bool isExpired)
    {
        var status = new List<string>();
        if (isDisabled) status.Add(AccountStatus.Disabled);
        if (isLocked) status.Add(AccountStatus.Locked);
        if (isExpired) status.Add(AccountStatus.Expired);
        return status.Count > 0 ? string.Join("&", status) : AccountStatus.Enabled;
    }

    private static void SetDefaultSecurityInfo(LdapUserInfo userInfo)
    {
        userInfo.Status = AccountStatus.Unknown;
        userInfo.PwdLastSet = PasswordStatus.Unknown;
        userInfo.PwdExpirationLeftDays = PasswordStatus.Unknown;
        userInfo.AccountExpires = null;
    }

    private static DateTime? GetAccountExpirationDate(string accountExpiresValue)
    {
        if (!long.TryParse(accountExpiresValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var accountExpiresLong)
            || accountExpiresLong == 0
            || accountExpiresLong == TimeConstants.NoExpiryDate)
        {
            return null;
        }

        try
        {
            return DateTime.FromFileTime(accountExpiresLong);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
