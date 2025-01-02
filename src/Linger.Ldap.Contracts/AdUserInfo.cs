namespace Linger.Ldap.Contracts;

public class AdUserInfo
{
    public string? DisplayName { get; set; }
    public string? SamAccountName { get; set; }
    public string? Upn { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Office { get; set; }
    public string? TelephoneNumber { get; set; }
    public string? WebPage { get; set; }
    public string? Description { get; set; }
    public DateTime? WhenCreated { get; set; }
    public string? Street { get; set; }
    public string? PostOfficeBox { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? HomePhone { get; set; }
    public string? Pager { get; set; }
    public string? Mobile { get; set; }
    public string? Fax { get; set; }
    public string? IpPhone { get; set; }
    public string? UserWorkstations { get; set; }
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? Title { get; set; }
    public string? Manager { get; set; }
    public string? UserType { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? PwdLastSet { get; set; }
    public string? LyncAddress { get; set; }
    public string? ProxyAddresses { get; set; }

    public string? ProfilePath { get; set; }

    //public string LogonScript { get; set; }
    public string? HomeDrive { get; set; }

    public string? HomeDirectory { get; set; }
    public string? Dn { get; set; }
    public string? ExtensionAttribute1 { get; set; }
    public string? Initials { get; set; }
    public string? ExMailboxDb { get; set; }
    public string[]? MemberOf { get; set; }
    public string? OtherTelephone { get; set; }
    public string? Status { get; set; }
    public string? AccountExpires { get; set; }
    public string? PwdExpirationLeftDays { get; set; }
}