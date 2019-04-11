using System;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Platform abstractions
    /// </summary>
    public static class Platform
    {
        private static readonly PlatformID[] _unixOperatingSystems =
        {
            PlatformID.Unix,
            PlatformID.MacOSX
        };

        private static PlatformID[] _windowsOperatingSystems =
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
                                      (_isUnixy = _unixOperatingSystems.Contains(Environment.OSVersion.Platform))
                                      ?? false;

        private static bool? _isUnixy;

        /// <summary>
        /// True when the current platform is one of the Windows variants
        /// </summary>
        public static bool IsWindows => _isWindows ??
                                        (_isWindows = _windowsOperatingSystems.Contains(Environment.OSVersion.Platform))
                                        ?? false;

        private static bool? _isWindows;
    }
}