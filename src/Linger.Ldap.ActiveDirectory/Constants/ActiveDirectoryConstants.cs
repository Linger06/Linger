namespace Linger.Ldap.ActiveDirectory.Constants;

internal static class ActiveDirectoryConstants
{
    public static class UserAccountControl
    {
        public const int Disabled = 0x0002;
        public const int PasswordNeverExpires = 0x10000;
    }

    public static class AccountStatus
    {
        public const string Enabled = "Enabled";
        public const string Disabled = "Disabled";
        public const string Locked = "Locked";
        public const string Expired = "Expired";
        public const string Unknown = "Unknown";
    }

    public static class PasswordStatus
    {
        public const string NeverExpires = "Password Never Expires";
        public const string NeverChanged = "Password has never been changed";
        public const string Unknown = "Unknown";
        public const string InvalidFormat = "Invalid format";
        public const string UnableToCalculate = "Unable to calculate";
        public const string NoExpirationPolicy = "No expiration policy";
        public const string NoExpirationSet = "No expiration set";
        public const string DomainPolicyNeverExpires = "Domain policy: Password Never Expires";
    }

    public static class TimeConstants
    {
        public const decimal TicksPerDay = 864000000000M;
        public const long NeverExpiresFlag = -9223372036854775808;
        public const long NoExpiryDate = 0x7FFFFFFFFFFFFFFF;
    }
}
