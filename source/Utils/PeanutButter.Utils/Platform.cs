using System;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Platform abstractions
    /// </summary>
    public static class Platform
    {
        private static readonly PlatformID[] UnixOperatingSystems =
        {
            PlatformID.Unix,
            PlatformID.MacOSX
        };

        private static readonly PlatformID[] WindowsOperatingSystems =
        {
            PlatformID.Win32NT,
            PlatformID.Xbox,
            PlatformID.Win32S,
            PlatformID.WinCE,
            PlatformID.Win32Windows
        };

        /// <summary>
        /// True when the current platform is Linux or OSX
        /// </summary>
        public static bool IsUnixy => _isUnixy ??
                                      (_isUnixy = UnixOperatingSystems.Contains(Environment.OSVersion.Platform))
                                      ?? false;

        private static bool? _isUnixy;

        /// <summary>
        /// True when the current platform is one of the Windows variants
        /// </summary>
        public static bool IsWindows => _isWindows ??
                                        (_isWindows = WindowsOperatingSystems.Contains(Environment.OSVersion.Platform))
                                        ?? false;

        private static bool? _isWindows;
        
        /// <summary>
        /// Are we running 64-bit? Note: you may be in a 32-bit runtime
        /// on a 64-bit machine...
        /// </summary>
        public static bool Is64Bit => IntPtr.Size == 8;
        /// <summary>
        /// Are we running 32-bit? Note: you may be in a 32-bit runtime
        /// on a 64-bit machine...
        /// </summary>
        public static bool Is32Bit => IntPtr.Size == 4;
    }
}