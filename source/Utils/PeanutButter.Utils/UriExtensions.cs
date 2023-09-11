using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Provides extensions for Uri objects and url strings
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class UriExtensions
{
    /// <summary>
    /// Find the root of the uri (schema, host, and, if required, port)
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static Uri Root(
        this Uri uri
    )
    {
        var host = uri.Host;
        var schema = uri.Scheme;
        var port = uri.Port;

        return uri.IsDefaultPort
            ? new Uri($"{schema}://{host}")
            : new Uri($"{schema}://{host}:{port}");
    }

    /// <summary>
    /// Convert the string to an Uri and return the Root()
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UriRoot(
        this string str
    )
    {
        return new Uri(str)
            .Root()
            .ToString()
            .TrimEnd('/');
    }
}