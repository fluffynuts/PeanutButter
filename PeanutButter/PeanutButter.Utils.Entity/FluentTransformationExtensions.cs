using System;

namespace EntityUtilities
{
    public static class FluentTransformationExtensions
    {
        // typical usage would be to tack this onto a FirstOrDefault()
        //  query from an IDbSet and supply your mapper function
        public static T2 Transform<T1, T2>(this T1 input, Func<T1, T2> transformer)
        {
            return transformer(input);
        }
    }
}