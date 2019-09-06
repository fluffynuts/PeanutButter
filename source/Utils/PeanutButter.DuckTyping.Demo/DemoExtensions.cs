using System;

namespace PeanutButter.DuckTyping.Demo
{
    public static class DemoExtensions
    {
        private static readonly Type StringType = typeof(string);
        public static bool IsString(this object ctx)
        {
            return ctx?.GetType() == StringType;
        }
    }
}