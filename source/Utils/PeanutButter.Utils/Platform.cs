using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Platform abstractions
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
    public
#endif
    static class Platform
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

    /// <summary>
    /// Provide the default path delimiter on this platform
    /// </summary>
    public static string PathDelimiter => IsUnixy
        ? "/"
        : "\\";
}

/// <summary>
/// Used to determine what runtime we're currently running on
/// - usually this is to distinguish between dotnet and mono,
/// but there could be other use-cases.
/// </summary>
public static class Runtime
{
    /// <summary>
    /// Returns true if the runtime is mono
    /// </summary>
    public static bool IsMono =>
        _isMono ??= Type.GetType("Mono.Runtime") is not null;

    private static bool? _isMono;

    /// <summary>
    /// Returns true if the runtime is .NET Framework
    /// </summary>
    public static bool IsNetFx =>
        _isNetFx ??= RuntimeInformation.FrameworkDescription.StartsWith(
            ".NET Framework",
            StringComparison.OrdinalIgnoreCase
        );

    private static bool? _isNetFx;

    /// <summary>
    /// Returns true if the runtime is dotnet core
    /// </summary>
    public static bool IsNetCore =>
        _isNetCore ??= NetCoreMatcher.IsMatch(RuntimeInformation.FrameworkDescription);

    private static readonly Regex NetCoreMatcher = new(
        "^.NET (Core|[\\d.]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );

    private static bool? _isNetCore;

    /// <summary>
    /// Returns true if the runtime is .NET Native
    /// </summary>
    public static bool IsNetNative =>
        _isNetNative ??= RuntimeInformation.FrameworkDescription.StartsWith(
            ".NET Native",
            StringComparison.OrdinalIgnoreCase
        );

    private static bool? _isNetNative;
}