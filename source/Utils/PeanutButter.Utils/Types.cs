using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    internal static class Types
    {
        public static readonly Type[] PrimitivesAndImmutables =
        {
            typeof(int),
            typeof(short),
            typeof(char),
            typeof(byte),
            typeof(long),
            typeof(string),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(DateTimeOffset),
            typeof(Guid)
        };
    }
}