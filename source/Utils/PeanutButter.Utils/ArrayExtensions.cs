using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    internal static class ArrayExtensions
    {
        internal static PropertyOrField[] Encapsulate(this PropertyInfo[] propertyInfos)
        {
            return propertyInfos.Select(p => new PropertyOrField(p)).ToArray();
        }

        internal static PropertyOrField[] Encapsulate(this FieldInfo[] propertyInfos)
        {
            return propertyInfos.Select(p => new PropertyOrField(p)).ToArray();
        }
    }
}