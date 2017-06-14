using System;
using System.ComponentModel;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum theEnumeration)
        {
            var fi = theEnumeration.GetType().GetField(theEnumeration.ToString());

            var attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            return theEnumeration.ToString();
        }
    }
}