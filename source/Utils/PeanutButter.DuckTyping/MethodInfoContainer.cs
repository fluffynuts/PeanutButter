using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Class which holds two lookups of method information: one accurate and one approximate
    /// </summary>
    public class MethodInfoContainer
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