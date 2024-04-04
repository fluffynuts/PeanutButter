using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

internal static class Program
{
    internal static void Main()
    {
        throw new Exception(
            "This method only exists to satiate compiler requirements for building against Microsoft.NET.Sdk.Web"
        );
    }
}