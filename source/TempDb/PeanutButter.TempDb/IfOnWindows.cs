using System;
using PeanutButter.Utils;

namespace PeanutButter.TempDb;

internal static class IfOnWindows
{
    ///
    /// Consts defined in WINBASE.H
    ///
    [Flags]
    internal enum MoveFileFlags
    {
        MOVEFILE_REPLACE_EXISTING = 1,
        MOVEFILE_COPY_ALLOWED = 2,

        MOVEFILE_DELAY_UNTIL_REBOOT =
            4, //This value can be used only if the process is in the context of a user who belongs to the administrators group or the LocalSystem account
        MOVEFILE_WRITE_THROUGH = 8
    }


    /// <summary>
    /// Marks the file for deletion during next system reboot
    /// </summary>
    /// <param name="lpExistingFileName">The current name of the file or directory on the local computer.</param>
    /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
    /// <param name="dwFlags">MoveFileFlags</param>
    /// <returns>bool</returns>
    /// <remarks>http://msdn.microsoft.com/en-us/library/aa365240(VS.85).aspx</remarks>
    [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "MoveFileEx")]
    internal static extern bool MoveFileEx(
        string lpExistingFileName,
        string lpNewFileName,
        MoveFileFlags dwFlags
    );

//Usage for marking the file to delete on reboot

    public static void RegisterToAutoDeleteAtNextStart(
        string path
    )
    {
        if (Platform.IsWindows)
        {
            MoveFileEx(path, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }
    }
}