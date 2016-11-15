using System;

namespace PeanutButter.Utils
{
    internal static class Types
    {
        public static readonly Type[] Primitives =
        {
            typeof(int),
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