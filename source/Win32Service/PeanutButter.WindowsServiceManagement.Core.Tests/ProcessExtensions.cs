using System.Diagnostics;
using System.Management;

namespace PeanutButter.WindowsServiceManagement.Core.Tests;

// TODO: this might be useful in PB.Utils; however this very variant is
// super-windows-specific, so one needs to come up with a better cross-platform
// variant; perhaps 'ps' for *nix?
public static class ProcessExtensions
{
    public static string QueryCommandline(
        this Process proc)
    {
        if (proc == null || proc.HasExited || proc.Id < 1)
        {
            return ""; // nothing we can do
        }

        var query = $"select CommandLine from Win32_Process where ProcessId={proc.Id}";
        var searcher = new ManagementObjectSearcher(query);
        var collection = searcher.Get();
        foreach (var o in collection)
        {
            return o["CommandLine"]?.ToString() ?? "";
        }

        return "";
    }
}