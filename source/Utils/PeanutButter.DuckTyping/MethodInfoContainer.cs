using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class MethodInfoContainer
    {
        public Dictionary<string, MethodInfo> MethodInfos { get; }
        public Dictionary<string, MethodInfo> FuzzyMethodInfos { get; }
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