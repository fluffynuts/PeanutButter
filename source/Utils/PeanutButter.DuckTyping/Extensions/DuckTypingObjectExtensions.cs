using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal class TypePair
    {
        public Type Type { get; set; }
        public Type FuzzyType { get; set; }
    }
    public static class DuckTypingObjectExtensions
    {
        public static bool CanDuckAs<T>(this object src)
        {
            return CanDuckAs<T>(src, false);
        }

        public static bool CanFuzzyDuckAs<T>(this object src)
        {
            return CanDuckAs<T>(src, true);
        }

        private static bool CanDuckAs<T>(this object src, bool allowFuzzy)
        {
            var type = typeof(T);
            var expectedProperties = allowFuzzy ? type.FindFuzzyProperties() :type.FindProperties();
            var srcType = src.GetType();
            var srcProperties = allowFuzzy ? srcType.FindFuzzyProperties() : srcType.FindProperties();
            if (!srcProperties.IsSuperSetOf(expectedProperties))
                return false;
            var expectedMethods = allowFuzzy ? type.FindFuzzyMethods() : type.FindMethods();
            var srcMethods = allowFuzzy ? srcType.FindFuzzyMethods() : srcType.FindMethods();
            return srcMethods.IsSuperSetOf(expectedMethods);
        }

        public static T DuckAs<T>(this object src) where T: class
        {
            return src.DuckAs<T>(false);
        }

        public static T FuzzyDuckAs<T>(this object src) where T: class
        {
            return src.DuckAs<T>(true);
        }

        internal static T DuckAs<T>(this object src, bool allowFuzzy) where T: class
        {
            if (src == null) return null;
            var duckable = allowFuzzy ? src.CanFuzzyDuckAs<T>() : src.CanDuckAs<T>();
            if (!duckable) return null;

            var duckType = FindOrCreateDuckTypeFor<T>(allowFuzzy);
            return (T)Activator.CreateInstance(duckType, src);
            
        }

        private static readonly Dictionary<Type, TypePair> _duckTypes 
            = new Dictionary<Type, TypePair>();

        private static Type FindOrCreateDuckTypeFor<T>(bool isFuzzy)
        {
            var key = typeof(T);
            lock(_duckTypes)
            {
                if (!_duckTypes.ContainsKey(key))
                {
                    _duckTypes[key] = new TypePair();
                }
                var match = _duckTypes[key];
                return isFuzzy ? GetFuzzyTypeFrom<T>(match) : GetTypeFrom<T>(match);
            }
        }

        private static Type GetTypeFrom<T>(TypePair match)
        {
            return match.Type ?? (match.Type = CreateDuckTypeFor<T>(false));
        }

        private static Type GetFuzzyTypeFrom<T>(TypePair match)
        {
            return match.FuzzyType ?? (match.FuzzyType = CreateDuckTypeFor<T>(true));
        }


        private static Type CreateDuckTypeFor<T>(bool isFuzzy)
        {
            var typeMaker = new TypeMaker();
            return isFuzzy
                    ? typeMaker.MakeFuzzyTypeImplementing<T>()
                    : typeMaker.MakeTypeImplementing<T>();
        }
    }
}
