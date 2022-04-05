using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    internal static class GatheringExtensions
    {
        internal static Dictionary<string, IHasValue> Add(
            this Dictionary<string, IHasValue> dict,
            string lastSwitch,
            string value
        )
        {
            var collection = dict.FindOrAdd(lastSwitch);
            collection.Add(value);
            return dict;
        }

        internal static Dictionary<string, IHasValue> Add(
            this Dictionary<string, IHasValue> dict,
            string sw
        )
        {
            dict.FindOrAdd(sw);
            return dict;
        }

        internal static IHasValue FindOrAdd(
            this Dictionary<string, IHasValue> dict,
            string sw
        )
        {
            if (dict.TryGetValue(sw, out var result))
            {
                return result;
            }

            result = new StringCollection();
            dict[sw] = result;
            return result;
        }
    }
}