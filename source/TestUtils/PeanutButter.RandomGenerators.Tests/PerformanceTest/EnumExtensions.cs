using System;
using System.ComponentModel;
using System.Linq;

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

            return attributes.Any()
                ? attributes[0].Description
                : theEnumeration.ToString();
        }
    }
}