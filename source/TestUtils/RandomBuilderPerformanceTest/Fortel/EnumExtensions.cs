using System;
using System.ComponentModel;
using System.Reflection;

namespace RandomBuilderPerformanceTest.Fortel
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum theEnumeration)
        {
            FieldInfo fi = theEnumeration.GetType().GetField(theEnumeration.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return theEnumeration.ToString();
        }
    }
}
