using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Static class to create instances of automatically generated
    /// types implementing the provided interfaces
    /// </summary>
    public static class Create
    {
        private static TypeMaker _typeMakerField;
        private static TypeMaker TypeMaker => _typeMakerField ?? (_typeMakerField = new TypeMaker());
        private static readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Creates an instance of a type implementing the interface T
        /// Will generate a type implementing T on the first call and
        /// re-use it on subsequent calls
        /// </summary>
        /// <typeparam name="T">Interface to implement in provided object</typeparam>
        /// <returns>Instance of an object implementing the provided interface</returns>
        public static T InstanceOf<T>()
        {
            return (T)CreateOrReuseInstanceOf(typeof(T), new List<object>());
        }

        private static object CreateOrReuseInstanceOf(
            Type toCreate,
            List<object> alreadyCreated)
        {
            var existing = alreadyCreated.FirstOrDefault(toCreate.IsInstanceOfType);
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

        private static readonly MethodInfo _getEmptyArrayGeneric = 
            typeof(Create).GetMethod("GetEmptyArrayOf", BindingFlags.Static | BindingFlags.NonPublic);
        // ReSharper disable once UnusedMember.Local
#pragma warning disable S1144
        private static T[] GetEmptyArrayOf<T>()
        {
#pragma warning restore D1144
            return new T[0];
        }

        private static void TrySetNewValue(object result, PropertyInfo pi)
        {
            var hasParameterlessConstructor = pi.PropertyType
                                                .GetConstructors()
                                                .Any(c => c.GetParameters().Length == 0);
            if (hasParameterlessConstructor)
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
