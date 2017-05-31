using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Comparers;

namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Provides a container for two property info lookups: one accurate and one approximate
    /// </summary>
    public class PropertyInfoContainer
    {
        /// <summary>
        /// Accurate property info lookup
        /// </summary>
        public Dictionary<string, PropertyInfo> PropertyInfos { get; }

        /// <summary>
        /// Approximate property info lookup
        /// </summary>
        public Dictionary<string, PropertyInfo> FuzzyPropertyInfos { get; }

        /// <summary>
        /// Constructs a new instance of the lookup container
        /// </summary>
        /// <param name="propertyInfos">PropertyInfos to contain</param>
        public PropertyInfoContainer(
            PropertyInfo[] propertyInfos
        )
        {
            var distinct = propertyInfos
                            .Distinct(new PropertyInfoComparer())
                            .ToArray();
            PropertyInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                Comparers.Comparers.NonFuzzyComparer
            );
            FuzzyPropertyInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                Comparers.Comparers.FuzzyComparer
            );
        }
    }
}