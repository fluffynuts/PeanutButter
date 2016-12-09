using System;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Base class for common shim functionality
    /// </summary>
    public abstract class ShimShamBase
    {
        private static readonly MethodInfo _getDefaultMethodGeneric = typeof(ShimShamBase).GetMethod("GetDefaultFor", BindingFlags.NonPublic | BindingFlags.Static);
        private TypeMaker _typeMaker;
        private readonly MethodInfo _genericMakeType = typeof(TypeMaker).GetMethod("MakeTypeImplementing");
        private readonly MethodInfo _genericFuzzyMakeType = typeof(TypeMaker).GetMethod("MakeFuzzyTypeImplementing");
        /// <summary>
        /// Gets the default value for a type
        /// </summary>
        /// <param name="correctType">Type to find the default value for</param>
        /// <returns>The value that would be returned by default(T) for that type</returns>
        public static object GetDefaultValueFor(Type correctType)
        {
            return _getDefaultMethodGeneric
                .MakeGenericMethod(correctType)
                .Invoke(null, null);

        }

        // ReSharper disable once UnusedMember.Local
#pragma warning disable S1144 // Unused private types or members should be removed
        private static T GetDefaultFor<T>()
        {
            return default(T);
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        /// <summary>
        /// Converts a property value from original type to another type using the provided converter
        /// </summary>
        /// <param name="converter">Converts the value</param>
        /// <param name="propValue">Value to convert</param>
        /// <param name="toType">Required output type</param>
        /// <returns>Value converted to required output type, where possible</returns>
        protected object ConvertWith(
            IConverter converter, 
            object propValue, 
            Type toType)
        {
            var convertMethod = converter.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(mi => mi.Name == "Convert" && mi.ReturnType == toType);
            // ReSharper disable once RedundantExplicitArrayCreation
            return convertMethod.Invoke(converter, new object[] { propValue });
        }

        /// <summary>
        /// Creates a new type to implement the requested interface type
        /// Used internally when fleshing out non-primitive properties
        /// </summary>
        /// <param name="type">Type to implement</param>
        /// <param name="isFuzzy">Flag to allow (or not) approximate / fuzzy ducking</param>
        /// <returns>Type implementing requested interface</returns>
        protected Type MakeTypeToImplement(Type type, bool isFuzzy)
        {
            var typeMaker = (_typeMaker ?? (_typeMaker = new TypeMaker()));
            var genericMethod = isFuzzy ? _genericFuzzyMakeType : _genericMakeType;
            var specific = genericMethod.MakeGenericMethod(type);
            return specific.Invoke(typeMaker, null) as Type;
        }

    }
}