using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Used to determine what runtime we're currently running on
/// - usually this is to distinguish between dotnet and mono,
/// but there could be other use-cases.
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class Runtime
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