using System;
using System.Collections.Generic;

namespace PeanutButter.DuckTyping
{
    public static class Create
    {
        private static TypeMaker _typeMakerField;
        private static TypeMaker TypeMaker => _typeMakerField ?? (_typeMakerField = new TypeMaker());
        private static readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

        public static T InstanceOf<T>()
        {
            var type = FindOrCreateTypeImplementing<T>();
            return (T)Activator.CreateInstance(type);
        }

        private static Type FindOrCreateTypeImplementing<T>()
        {
            lock(_typeCache)
            {
                Type implemented;
                var typeToImplement = typeof(T);
                if (!_typeCache.TryGetValue(typeToImplement, out implemented))
                {
                    implemented = TypeMaker.MakeTypeImplementing<T>();
                    _typeCache[typeToImplement] = implemented;
                }
                return implemented;
            }
        }
    }
}
