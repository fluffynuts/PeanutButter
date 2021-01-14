using System.Collections.Generic;

namespace PeanutButter.EasyArgs
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