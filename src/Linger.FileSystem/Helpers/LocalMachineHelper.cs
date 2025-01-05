using System.Net;

namespace Linger.FileSystem.Helpers;

public class LocalMachineHelper
{
    /*
        https://stackoverflow.com/questions/1768198/how-do-i-get-the-computer-name-in-net
        https://stackoverflow.com/questions/1233217/difference-between-systeminformation-computername-environment-machinename-and
     */

    public static string ServerDetails()
    {
        var machineName = string.Empty;
        var hostName = string.Empty;
        var computerName = string.Empty;
        try
        {
            machineName = Environment.MachineName;
            hostName = Dns.GetHostName();
            computerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
        catch
        {
            // ignored
        }

        var details = $"MachineName:'{machineName}' HostName:'{hostName}' ComputerName:'{computerName}'";
        return details;
    }
}