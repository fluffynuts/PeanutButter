using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Imported.PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Comparers
#else
namespace PeanutButter.DuckTyping.Comparers
#endif
{
    /// <summary>
    /// Class which holds two lookups of method information: one accurate and one approximate
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class MethodInfoContainer
    {
        /// <summary>
        /// Accurate dictionary of method information
        /// </summary>
        public Dictionary<string, MethodInfo[]> MethodInfos { get; }

        /// <summary>
        /// Approximate dictionary of method information
        /// </summary>
        public Dictionary<string, MethodInfo[]> FuzzyMethodInfos { get; }

        /// <summary>
        /// Constructs a new instance of the MethodInfo container
        /// </summary>
        /// <param name="methodInfos">MethodInfos to contain</param>
        public MethodInfoContainer(
            MethodInfo[] methodInfos
        )
        {
            MethodInfos = new Dictionary<string, MethodInfo[]>(Comparers.NonFuzzyComparer);
            FuzzyMethodInfos = new Dictionary<string, MethodInfo[]>(Comparers.FuzzyComparer);
            foreach (var methodInfo in methodInfos)
            {
                AddToArrayValue(MethodInfos, methodInfo.Name, methodInfo);
                AddToArrayValue(FuzzyMethodInfos, methodInfo.Name, methodInfo);
            }
        }

        private static void AddToArrayValue(
            IDictionary<string, MethodInfo[]> dict,
            string key,
            MethodInfo methodInfo
        )
        {
            if (!dict.TryGetValue(key, out var existing))
            {
                dict[key] = new[] { methodInfo };
                return;
            }
            dict[key] = existing.And(methodInfo);
        }
    }
}