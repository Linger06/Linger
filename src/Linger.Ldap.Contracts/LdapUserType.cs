namespace Linger.Ldap.Contracts;

public static class LdapUserType
{
    #region 基本标识信息
    public const string DisplayName = "displayName";
    public const string SamAccountName = "sAMAccountName";
    public const string Upn = "userPrincipalName";
    public const string Name = "name";
    public const string Dn = "distinguishedName";
    #endregion

    #region 个人信息
    public const string FirstName = "givenName";
    public const string LastName = "sn";
    public const string Initials = "initials";
    public const string Description = "description";
    public const string WhenCreated = "whenCreated";
    #endregion

    #region 联系信息
    // 电子邮件
    public const string Email = "mail";
    public const string LyncAddress = "msRTCSIP-PrimaryUserAddress";
    public const string ProxyAddresses = "proxyAddresses";
    public const string WebPage = "wWWHomePage";

    // 电话号码
    public const string TelephoneNumber = "telephoneNumber";
    public const string Mobile = "mobile";
    public const string HomePhone = "homePhone";
    public const string Pager = "pager";
    public const string Fax = "facsimileTelephoneNumber";
    public const string IpPhone = "ipPhone";
    public const string OtherTelephone = "otherTelephone";
    #endregion

    #region 组织信息
    public const string Company = "company";
    public const string Department = "department";
    public const string Title = "title";
    public const string Manager = "manager";
    public const string Office = "physicalDeliveryOfficeName";
    public const string EmployeeId = "employeeID";
    public const string EmployeeNumber = "employeeNumber";
    #endregion

    #region 地址信息
    public const string Street = "streetAddress";
    public const string PostOfficeBox = "postOfficeBox";
    public const string City = "l";
    public const string State = "st";
    public const string PostalCode = "postalCode";
    public const string Country = "co";
    #endregion

    #region 系统信息
    public const string UserWorkstations = "userWorkstations";
    public const string ProfilePath = "profilePath";
    public const string HomeDrive = "homeDrive";
    public const string HomeDirectory = "homeDirectory";
    public const string ExMailboxDb = "homeMDB";
    public const string ExtensionAttribute1 = "extensionAttribute1";
    public const string MemberOf = "memberOf";
    #endregion

    #region 安全信息
    public const string AccountExpires = "accountExpires";
    public const string UserAccountControl = "userAccountControl";
    public const string PwdLastSet = "pwdLastSet";
    #endregion
}