using System.ComponentModel;

namespace Linger.UnitTests;

public enum StatusCode
{
    [Description("Deleted")] Deleted = -1,
    [Description("Enable")] Enable = 0,
    [Description("Disable")] Disable = 1
}