using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
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