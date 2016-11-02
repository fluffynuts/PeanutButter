using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping
{
    public static class Create
    {
        private static TypeMaker _typeMakerField;
        private static TypeMaker TypeMaker => _typeMakerField ?? (_typeMakerField = new TypeMaker());
        private static readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

        public static T InstanceOf<T>()
        {
            return (T)CreateOrReuseInstanceOf(typeof(T), new List<object>());
        }

        private static object CreateOrReuseInstanceOf(
            Type toCreate,
            List<object> alreadyCreated)
        {
            var existing = alreadyCreated.FirstOrDefault(o => toCreate.IsInstanceOfType(o));
            if (existing != null)
                return existing;
            var type = FindOrCreateTypeImplementing(toCreate);
            var result = Activator.CreateInstance(type);
            alreadyCreated.Add(result);
            var resultProps = result.GetType().GetProperties();
            var complexProps = resultProps
                                    .Where(p => !p.PropertyType.ShouldTreatAsPrimitive())
                                    .ToArray();
            foreach (var p in complexProps)
            {
                if (!p.CanWrite)
                    continue;
                var toAssign = CreateOrReuseInstanceOf(p.PropertyType, alreadyCreated);
                p.SetValue(result, toAssign);
            }
            return result;
        }

        private static readonly MethodInfo _genericMake = typeof(TypeMaker)
                                                    .GetMethods()
                                                    .FirstOrDefault(mi => mi.Name == "MakeTypeImplementing" && mi.IsGenericMethod && mi.GetParameters().Length == 0);

        private static Type FindOrCreateTypeImplementing(Type typeToImplement)
        {
            Type implemented;
            if (!_typeCache.TryGetValue(typeToImplement, out implemented))
            {
                var method = _genericMake.MakeGenericMethod(typeToImplement);
                implemented = (Type)method.Invoke(TypeMaker, new object[0]);
                _typeCache[typeToImplement] = implemented;
            }
            return implemented;
        }
    }
}
