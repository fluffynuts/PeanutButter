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
            var propInfos = result.GetType().GetProperties();
            CreateComplexPropertyValues(alreadyCreated, propInfos, result);
            CreateConcretePropertyValuesOn(result, propInfos);
            return result;
        }

        private static void CreateConcretePropertyValuesOn(
            object result,
            PropertyInfo[] propInfos)
        {
            foreach (var pi in propInfos.Where(pi => !pi.PropertyType.IsInterface))
            {
                if (pi.PropertyType.IsArray)
                {
                    TrySetArrayValue(result, pi);
                }
                else
                {
                    TrySetNewValue(result, pi);
                }
            }
        }

        private static void TrySetArrayValue(object result, PropertyInfo pi)
        {
            var specific = _getEmptyArrayGeneric.MakeGenericMethod(pi.PropertyType.GetElementType());
            var empty = specific.Invoke(null, new object[] { } );
            pi.SetValue(result, empty);
        }

        private static MethodInfo _getEmptyArrayGeneric = typeof(Create).GetMethod("GetEmptyArrayOf", BindingFlags.Static | BindingFlags.NonPublic);
        private static T[] GetEmptyArrayOf<T>()
        {
            return new T[0];
        }

        private static void TrySetNewValue(object result, PropertyInfo pi)
        {
            TryDo(() => pi.SetValue(result, Activator.CreateInstance(pi.PropertyType)));
        }

        private static void TryDo(Action action)
        {
            try
            {
                action();
            }
            catch { /* intentionally left blank */ }
        }

        private static void CreateComplexPropertyValues(List<object> alreadyCreated, PropertyInfo[] propertyInfos, object result)
        {
            var resultProps = propertyInfos;
            var complexProps = resultProps
                .Where(p => !p.PropertyType.ShouldTreatAsPrimitive() && p.PropertyType.IsInterface)
                .ToArray();
            foreach (var p in complexProps)
            {
                if (!p.CanWrite)
                    continue;
                var toAssign = CreateOrReuseInstanceOf(p.PropertyType, alreadyCreated);
                p.SetValue(result, toAssign);
            }
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
