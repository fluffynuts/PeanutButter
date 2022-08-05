using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        public Dictionary<string, MethodInfo> MethodInfos { get; }

        /// <summary>
        /// Approximate dictionary of method information
        /// </summary>
        public Dictionary<string, MethodInfo> FuzzyMethodInfos { get; }

        /// <summary>
        /// Constructs a new instance of the MethodInfo container
        /// </summary>
        /// <param name="methodInfos">MethodInfos to contain</param>
        public MethodInfoContainer(
            MethodInfo[] methodInfos
        )
        {
            var distinct = methodInfos.Distinct(new MethodInfoComparer()).ToArray();
            MethodInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                Comparers.NonFuzzyComparer
            );
            FuzzyMethodInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                Comparers.FuzzyComparer
            );
        }
    }
}