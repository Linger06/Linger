using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
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

        if (userPrincipal.GetUnderlyingObject() is not System.DirectoryServices.DirectoryEntry directoryEntry) return null;

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
        MapSystemInfo(userInfo, directoryEntry);

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

    private static void MapSystemInfo(AdUserInfo userInfo, System.DirectoryServices.DirectoryEntry entry)
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
            const decimal TicksPerDay = 864000000000M; // 24 * 60 * 60 * 10000000 (一天的100纳秒数)
            var maxPwdAgeDays = Math.Abs(maxPwdAge.Value) / TicksPerDay;

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

    public static AdUserInfo ToAdUserInfo(this SearchResult result)
    {
        PropertyInfo[] properties = typeof(AdUserInfo).GetProperties();

        var userInfo = new AdUserInfo();

        foreach (PropertyInfo propertyInfo in properties)
        {
            var flag = propertyInfo.Name == "DisplayName";
            if (flag)
            {
                var flag2 = result.Properties["DisplayName"].Count > 0;
                userInfo.DisplayName = flag2 ? result.Properties["DisplayName"][0].ToString() : "NULL";
            }

            userInfo.SamAccountName = result.Properties["samAccountName"][0].ToString();
            var flag3 = propertyInfo.Name == "UPN";
            if (flag3)
            {
                var flag4 = result.Properties["userPrincipalName"].Count > 0;
                if (flag4)
                {
                    userInfo.Upn = result.Properties["userprincipalname"][0].ToString();
                }
                else
                {
                    userInfo.Upn = "NULL";
                }
            }

            var flag10 = propertyInfo.Name == "FirstName";
            if (flag10)
            {
                var flag11 = result.Properties["givenName"].Count > 0;
                if (flag11)
                {
                    userInfo.FirstName = result.Properties["givenName"][0].ToString();
                }
                else
                {
                    userInfo.FirstName = "NULL";
                }
            }

            var flag12 = propertyInfo.Name == "LastName";
            if (flag12)
            {
                var flag13 = result.Properties["sn"].Count > 0;
                if (flag13)
                {
                    userInfo.LastName = result.Properties["sn"][0].ToString();
                }
                else
                {
                    userInfo.LastName = "NULL";
                }
            }

            var flag14 = propertyInfo.Name == "name";
            if (flag14)
            {
                var flag15 = result.Properties["name"].Count > 0;
                userInfo.Name = flag15 ? result.Properties["name"][0].ToString() : "NULL";
            }

            var flag16 = propertyInfo.Name == "Initials";
            if (flag16)
            {
                var flag17 = result.Properties["initials"].Count > 0;
                if (flag17)
                {
                    userInfo.Initials = result.Properties["initials"][0].ToString();
                }
                else
                {
                    userInfo.Initials = "NULL";
                }
            }

            var flag18 = propertyInfo.Name == "Description";
            if (flag18)
            {
                var flag19 = result.Properties["description"].Count > 0;
                if (flag19)
                {
                    userInfo.Description = result.Properties["description"][0].ToString();
                }
                else
                {
                    userInfo.Description = "NULL";
                }
            }

            var flag20 = propertyInfo.Name == "Office";
            if (flag20)
            {
                var flag21 = result.Properties["physicalDeliveryOfficeName"].Count > 0;
                if (flag21)
                {
                    userInfo.Office = result.Properties["physicalDeliveryOfficeName"][0].ToString();
                }
                else
                {
                    userInfo.Office = "NULL";
                }
            }

            var flag22 = propertyInfo.Name == "TelephoneNumber";
            if (flag22)
            {
                var flag23 = result.Properties["telephoneNumber"].Count > 0;
                if (flag23)
                {
                    userInfo.TelephoneNumber = result.Properties["telephoneNumber"][0].ToString();
                }
                else
                {
                    userInfo.TelephoneNumber = "NULL";
                }
            }

            var flag24 = propertyInfo.Name == "OtherTelephone";
            if (flag24)
            {
                var flag25 = result.Properties["OtherTelephone"].Count > 1;
                if (flag25)
                {
                    for (var i = 0; i < result.Properties["OtherTelephone"].Count; i++)
                    {
                        userInfo.OtherTelephone =
                            userInfo.OtherTelephone + result.Properties["OtherTelephone"][i] + "^";
                    }
                }
                else
                {
                    var flag26 = result.Properties["OtherTelephone"].Count == 1;
                    if (flag26)
                    {
                        for (var j = 0; j < result.Properties["OtherTelephone"].Count; j++)
                        {
                            userInfo.OtherTelephone = result.Properties["OtherTelephone"][j].ToString();
                        }
                    }
                    else
                    {
                        userInfo.OtherTelephone = "NULL";
                    }
                }
            }

            var flag27 = propertyInfo.Name == "Email";
            if (flag27)
            {
                var flag28 = result.Properties["mail"].Count > 0;
                if (flag28)
                {
                    userInfo.Email = result.Properties["mail"][0].ToString();
                }
                else
                {
                    userInfo.Email = "NULL";
                }
            }

            var flag29 = propertyInfo.Name == "WebPage";
            if (flag29)
            {
                var flag30 = result.Properties["wWWHomePage"].Count > 0;
                if (flag30)
                {
                    userInfo.WebPage = result.Properties["wWWHomePage"][0].ToString();
                }
                else
                {
                    userInfo.WebPage = "NULL";
                }
            }

            var flag31 = propertyInfo.Name == "WhenCreated";
            if (flag31)
            {
                var flag32 = result.Properties["WhenCreated"].Count > 0;
                if (flag32)
                {
                    var createdDate = result.Properties["WhenCreated"][0].ToString();
                    userInfo.WhenCreated = DateTime.Parse(createdDate!).ToLocalTime();
                }
                else
                {
                    userInfo.WhenCreated = null;
                }
            }

            var flag33 = propertyInfo.Name == "Street";
            if (flag33)
            {
                var flag34 = result.Properties["streetAddress"].Count > 0;
                if (flag34)
                {
                    userInfo.Street = result.Properties["streetAddress"][0].ToString();
                }
                else
                {
                    userInfo.Street = "NULL";
                }
            }

            var flag35 = propertyInfo.Name == "PostOfficeBox";
            if (flag35)
            {
                var flag36 = result.Properties["postOfficeBox"].Count > 0;
                if (flag36)
                {
                    userInfo.PostOfficeBox = result.Properties["postOfficeBox"][0].ToString();
                }
                else
                {
                    userInfo.PostOfficeBox = "NULL";
                }
            }

            var flag37 = propertyInfo.Name == "City";
            if (flag37)
            {
                var flag38 = result.Properties["l"].Count > 0;
                if (flag38)
                {
                    userInfo.City = result.Properties["l"][0].ToString();
                }
                else
                {
                    userInfo.City = "NULL";
                }
            }

            var flag39 = propertyInfo.Name == "State";
            if (flag39)
            {
                var flag40 = result.Properties["st"].Count > 0;
                if (flag40)
                {
                    userInfo.State = result.Properties["st"][0].ToString();
                }
                else
                {
                    userInfo.State = "NULL";
                }
            }

            var flag41 = propertyInfo.Name == "PostalCode";
            if (flag41)
            {
                var flag42 = result.Properties["postalCode"].Count > 0;
                if (flag42)
                {
                    userInfo.PostalCode = result.Properties["postalCode"][0].ToString();
                }
                else
                {
                    userInfo.PostalCode = "NULL";
                }
            }

            var flag43 = propertyInfo.Name == "Country";
            if (flag43)
            {
                var flag44 = result.Properties["co"].Count > 0;
                if (flag44)
                {
                    userInfo.Country = result.Properties["co"][0].ToString();
                }
                else
                {
                    userInfo.Country = "NULL";
                }
            }

            var flag45 = propertyInfo.Name == "HomePhone";
            if (flag45)
            {
                var flag46 = result.Properties["homePhone"].Count > 0;
                if (flag46)
                {
                    userInfo.HomePhone = result.Properties["homePhone"][0].ToString();
                }
                else
                {
                    userInfo.HomePhone = "NULL";
                }
            }

            var flag47 = propertyInfo.Name == "Pager";
            if (flag47)
            {
                var flag48 = result.Properties["Pager"].Count > 0;
                if (flag48)
                {
                    userInfo.Pager = result.Properties["Pager"][0].ToString();
                }
                else
                {
                    userInfo.Pager = "NULL";
                }
            }

            var flag49 = propertyInfo.Name == "Mobile";
            if (flag49)
            {
                var flag50 = result.Properties["mobile"].Count > 0;
                if (flag50)
                {
                    userInfo.Mobile = result.Properties["mobile"][0].ToString();
                }
                else
                {
                    userInfo.Mobile = "NULL";
                }
            }

            var flag51 = propertyInfo.Name == "Fax";
            if (flag51)
            {
                var flag52 = result.Properties["facsimileTelephoneNumber"].Count > 0;
                if (flag52)
                {
                    userInfo.Fax = result.Properties["facsimileTelephoneNumber"][0].ToString();
                }
                else
                {
                    userInfo.Fax = "NULL";
                }
            }

            var flag53 = propertyInfo.Name == "IPPhone";
            if (flag53)
            {
                var flag54 = result.Properties["ipPhone"].Count > 0;
                if (flag54)
                {
                    userInfo.IpPhone = result.Properties["ipPhone"][0].ToString();
                }
                else
                {
                    userInfo.IpPhone = "NULL";
                }
            }

            var flag55 = propertyInfo.Name == "UserWorkstations";
            if (flag55)
            {
                var flag56 = result.Properties["UserWorkstations"].Count > 0;
                if (flag56)
                {
                    userInfo.UserWorkstations = result.Properties["UserWorkstations"][0].ToString();
                }
                else
                {
                    userInfo.UserWorkstations = "NULL";
                }
            }

            var flag57 = propertyInfo.Name == "Company";
            if (flag57)
            {
                var flag58 = result.Properties["company"].Count > 0;
                if (flag58)
                {
                    userInfo.Company = result.Properties["company"][0].ToString();
                }
                else
                {
                    userInfo.Company = "NULL";
                }
            }

            var flag59 = propertyInfo.Name == "Department";
            if (flag59)
            {
                var flag60 = result.Properties["department"].Count > 0;
                if (flag60)
                {
                    userInfo.Department = result.Properties["department"][0].ToString();
                }
                else
                {
                    userInfo.Department = "NULL";
                }
            }

            var flag61 = propertyInfo.Name == "Title";
            if (flag61)
            {
                var flag62 = result.Properties["Title"].Count > 0;
                if (flag62)
                {
                    userInfo.Title = result.Properties["Title"][0].ToString();
                }
                else
                {
                    userInfo.Title = "NULL";
                }
            }

            //var flag63 = propertyInfo.Name == "Manager";
            //if (flag63)
            //{
            //    var flag64 = result.Properties["Manager"].Count > 0;
            //    if (flag64)
            //    {
            //        SearchResult? searchResult = SearchManager(result);
            //        if (searchResult != null)
            //        {
            //            var value = searchResult.Properties["displayName"][0].ToString();
            //            userInfo.Manager = value;
            //        }
            //        else
            //        {
            //            userInfo.Manager = "NULL";
            //        }
            //    }
            //    else
            //    {
            //        userInfo.Manager = "NULL";
            //    }
            //}

            var flag65 = propertyInfo.Name == "UserType";
            if (flag65)
            {
                var flag66 = result.Properties["homeMDB"].Count > 0;
                if (flag66)
                {
                    userInfo.UserType = "UserMailbox";
                }
                else
                {
                    userInfo.UserType = "User";
                }
            }

            var flag69 = propertyInfo.Name == "EmployeeID";
            if (flag69)
            {
                var flag70 = result.Properties["EmployeeID"].Count > 0;
                if (flag70)
                {
                    userInfo.EmployeeId = result.Properties["EmployeeID"][0].ToString();
                }
                else
                {
                    userInfo.EmployeeId = "NULL";
                }
            }

            var flag71 = propertyInfo.Name == "EmployeeNumber";
            if (flag71)
            {
                var flag72 = result.Properties["EmployeeNumber"].Count > 0;
                if (flag72)
                {
                    userInfo.EmployeeNumber = result.Properties["EmployeeNumber"][0].ToString();
                }
                else
                {
                    userInfo.EmployeeNumber = "NULL";
                }
            }

            var flag73 = propertyInfo.Name == "PwdLastSet";
            if (flag73)
            {
                var flag74 = result.Properties["PwdLastSet"].Count > 0;
                if (flag74)
                {
                    var flag75 = result.Properties["PwdLastSet"][0].ToString() != "0";
                    if (flag75)
                    {
                        userInfo.PwdLastSet =
                            DateTime.FromFileTime((long)result.Properties["PwdLastSet"][0]).ToString();
                    }
                    else
                    {
                        userInfo.PwdLastSet = "Password has never been changed.";
                    }
                }
                else
                {
                    userInfo.PwdLastSet = "NULL";
                }
            }

            var flag82 = propertyInfo.Name == "LyncAddress";
            if (flag82)
            {
                var flag83 = result.Properties["msRTCSIP-PrimaryUserAddress"].Count > 0;
                if (flag83)
                {
                    userInfo.LyncAddress = result.Properties["msRTCSIP-PrimaryUserAddress"][0].ToString();
                }
                else
                {
                    userInfo.LyncAddress = "NULL";
                }
            }

            var flag84 = propertyInfo.Name == "ProxyAddresses";
            if (flag84)
            {
                var flag85 = result.Properties["ProxyAddresses"].Count > 0;
                if (flag85)
                {
                    for (var k = 0; k < result.Properties["ProxyAddresses"].Count; k++)
                    {
                        userInfo.ProxyAddresses =
                            userInfo.ProxyAddresses + result.Properties["ProxyAddresses"][k] + " ^ ";
                    }
                }
                else
                {
                    userInfo.ProxyAddresses = "NULL";
                }
            }

            var flag86 = propertyInfo.Name == "ProfilePath";
            if (flag86)
            {
                var flag87 = result.Properties["ProfilePath"].Count > 0;
                if (flag87)
                {
                    userInfo.ProfilePath = result.Properties["ProfilePath"][0].ToString();
                }
                else
                {
                    userInfo.ProfilePath = "NULL";
                }
            }

            var flag88 = propertyInfo.Name == "HomeDrive";
            if (flag88)
            {
                var flag89 = result.Properties["HomeDrive"].Count > 0;
                if (flag89)
                {
                    userInfo.HomeDrive = result.Properties["HomeDrive"][0].ToString();
                }
                else
                {
                    userInfo.HomeDrive = "NULL";
                }
            }

            var flag90 = propertyInfo.Name == "HomeDirectory";
            if (flag90)
            {
                var flag91 = result.Properties["HomeDirectory"].Count > 0;
                if (flag91)
                {
                    userInfo.HomeDirectory = result.Properties["HomeDirectory"][0].ToString();
                }
                else
                {
                    userInfo.HomeDirectory = "NULL";
                }
            }

            var flag92 = propertyInfo.Name == "ExMailboxDB";
            if (flag92)
            {
                var flag93 = result.Properties["homeMDB"].Count > 0;
                if (flag93)
                {
                    userInfo.ExMailboxDb = result.Properties["homeMDB"][0].ToString();
                }
                else
                {
                    userInfo.ExMailboxDb = "NULL";
                }
            }

            var flag94 = propertyInfo.Name == "DN";
            if (flag94)
            {
                var flag95 = result.Properties["distinguishedName"].Count > 0;
                if (flag95)
                {
                    userInfo.Dn = result.Properties["distinguishedName"][0].ToString();
                }
                else
                {
                    userInfo.Dn = "NULL";
                }
            }

            var flag96 = propertyInfo.Name == "ExtensionAttribute1";
            if (flag96)
            {
                var flag97 = result.Properties["ExtensionAttribute1"].Count > 0;
                if (flag97)
                {
                    userInfo.ExtensionAttribute1 = result.Properties["ExtensionAttribute1"][0].ToString();
                }
                else
                {
                    userInfo.ExtensionAttribute1 = "NULL";
                }
            }

            //var flag98 = propertyInfo.Name == "MemberOf";
            //if (flag98)
            //{
            //    var flag99 = result.Properties["MemberOf"].Count > 0;
            //    if (flag99)
            //    {
            //        var count = result.Properties["memberOf"].Count;
            //        var text2 = string.Empty;
            //        for (var k = 0; k < count; k++)
            //        {
            //            var groupDn = directoryEntry.Properties["memberOf"][k].ToString();
            //            //new ADHelper2() { }
            //            //SearchResult searchResult2 =  searchGroupDN(groupDN);
            //            //string text3 = searchResult2.Properties["samAccountName"][0].ToString();
            //            text2 = text2 + groupDn + ";";
            //        }

            //        userInfo.MemberOf = text2;
            //    }
            //    else
            //    {
            //        userInfo.MemberOf = "NULL";
            //    }
            //}

            var maxPwdDays = 0L;
            var num = 2;
            var directoryEntry = new DirectoryEntry { Path = result.Path };
            //此属性要有更高等级的权限才可以访问，普通账号的权限访问会报异常
            var propertyName = "userAccountControl";
            var df = result.Properties.Contains(propertyName);
            if (df)
            {
                var flag5 = propertyInfo.Name == nameof(AdUserInfo.Status);
                if (flag5)
                {
                    var flag6 = Convert.ToBoolean(directoryEntry.InvokeGet("IsAccountLocked"));

                    var flag7 = ((int)result.Properties[propertyName][0] & num) > 0;
                    if (flag7)
                    {
                        userInfo.Status = "Disabled";
                    }
                    else
                    {
                        var flag8 = flag6;
                        if (flag8)
                        {
                            userInfo.Status = "Locked";
                        }
                        else
                        {
                            var flag9 = (((int)result.Properties[propertyName][0] & num) > 0) & flag6;
                            userInfo.Status = flag9 ? "Disabled&Locked" : "Normal";
                        }
                    }
                }

                //不再判断此属性
                var flag67 = propertyInfo.Name == "AccountExpires";


                var flag76 = propertyInfo.Name == "PwdExpirationLeftDays";
                if (flag76)
                {
                    var num2 = (int)result.Properties[propertyName][0];
                    var num3 = 65536;
                    var flag77 = Convert.ToBoolean(num2 & num3);
                    if (flag77)
                    {
                        userInfo.PwdExpirationLeftDays = "Set Account Options to Password Never Expires.";
                    }
                    else
                    {
                        var flag78 = maxPwdDays == 10675199L;
                        if (flag78)
                        {
                            userInfo.PwdExpirationLeftDays = "Set Group Policy to Password Never Expires.";
                        }
                        else
                        {
                            var flag79 = result.Properties["PwdLastSet"][0].ToString() != "0";
                            if (flag79)
                            {
                                var obj = result.Properties["PwdLastSet"][0];
                                var num4 = maxPwdDays -
                                           DateTime.Today.Subtract(DateTime.FromFileTime((long)obj)).Days;
                                var flag80 = num4 >= 0L;
                                if (flag80)
                                {
                                    userInfo.PwdExpirationLeftDays = string.Concat("MaxPwdAge: ", maxPwdDays,
                                        ", DaysLeft: ", num4.ToString());
                                }
                                else
                                {
                                    userInfo.PwdExpirationLeftDays = string.Concat("MaxPwdAge: ", maxPwdDays,
                                        ", DaysExpired: ", (-num4).ToString());
                                }
                            }
                            else
                            {
                                var flag81 = result.Properties["PwdLastSet"][0].ToString() == "0";
                                if (flag81)
                                {
                                    userInfo.PwdExpirationLeftDays = "Password has never been changed.";
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                userInfo.Status = "Unknow";
                userInfo.PwdExpirationLeftDays = "Unknow";
            }
        }
        return userInfo;
    }

    public static List<AdUserInfo> ToAdUsersInfo(this SearchResultCollection resultCollection)
    {
        var userList = new List<AdUserInfo>();

        foreach (SearchResult result in resultCollection)
        {
            var userInfo = result.ToAdUserInfo();
            if (userInfo != null)
            {
                userList.Add(userInfo);
            }
        }

        return userList;
    }

    //private static SearchResult? SearchManager(SearchResult dn)
    //{
    //    var keyValuePairs = new Dictionary<string, string>();
    //    if (dn.Properties["Manager"].Count > 0)
    //    {
    //        var name = dn.Properties["Manager"][0].ToString();
    //        if (name == null)
    //        {
    //            throw new ArgumentNullException(nameof(name));
    //        }

    //        keyValuePairs.Add("distinguishedName", name);
    //        SearchResult? result = GetAdUser(keyValuePairs);
    //        return result;
    //    }

    //    return null;
    //}


}