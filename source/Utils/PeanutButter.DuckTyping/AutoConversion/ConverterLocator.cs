using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
using Imported.PeanutButter.DuckTyping.AutoConversion.Converters;
#else
using PeanutButter.DuckTyping.AutoConversion.Converters;
#endif

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion
#else
namespace PeanutButter.DuckTyping.AutoConversion
#endif
{
    internal static class ConverterLocator
    {
        internal static IDictionary<Tuple<Type, Type>, IConverter> Converters =>
            _converters ??= FindConverters();

        private static Dictionary<Tuple<Type, Type>, IConverter>
            _converters;

        private static Dictionary<Tuple<Type, Type>, IConverter> FindConverters()
        {
            var converterTypes = FindConverterTypes();
            var temp = converterTypes
                .Select(TryConstruct)
                .Where(c => c != null)
                .Union(MakeStringConverters())
                .Union(MakeStringArrayConverters())
                .Union(MakeNullableStringConverters())
                .Where(converter => converter.GetType().Implements(typeof(IConverter<,>)))
                .Select(converter =>
                {
                    // what if a converter implements multiple IConverter<,> interfaces?
                    try
                    {
                        var iface = converter.GetType().GetInterfaces()
                            .Single(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IConverter<,>)
                            );
                        var types = iface.GetGenericArguments();
                        return new[]
                        {
                            new { key = Tuple.Create(types[0], types[1]), value = converter },
                            new { key = Tuple.Create(types[1], types[0]), value = converter }
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .SelectMany(o => o)
                .Where(o => o is not null);

            var result = new Dictionary<Tuple<Type, Type>, IConverter>();
            foreach (var o in temp)
            {
                try
                {
                    result.Add(o.key, o.value);
                }
                catch
                {
                    Trace.WriteLine(
                        $@"WARNING: Converter {
                            result[o.key]
                        } will be used for converting between {
                            o.key.Item1
                        } and {
                            o.key.Item2
                        } (discarding instance of {
                            o.value.GetType()
                        })"
                    );
                }
            }

            return result;
        }

        private static IEnumerable<IConverter> MakeNullableStringConverters()
        {
            var genericType = typeof(GenericNullableStringConverter<>);
            return new[]
            {
                typeof(bool),
                typeof(DateTime),
                typeof(TimeSpan)
            }.Union(NumericTypes).Select(type =>
            {
                var converterType = genericType.MakeGenericType(type);
                return (IConverter) Activator.CreateInstance(converterType);
            }).ToArray();
        }

        private static readonly Type[] NumericTypes =
        {
            typeof(byte),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        private static IConverter[] MakeStringConverters()
        {
            var types = FindTypesWhichCanTryParseStrings();
            var genericType = typeof(GenericStringConverter<>);
            var converters = new List<IConverter>();
            foreach (var type in types)
            {
                TryAddConverterWith(genericType, type, converters);
            }

            return converters.ToArray();
        }

        private static IConverter[] MakeStringArrayConverters()
        {
            var types = FindTypesWhichCanTryParseStrings();
            var genericType = typeof(GenericStringStringArrayConverter<>);
            var converters = new List<IConverter>();
            foreach (var type in types)
            {
                TryAddConverterWith(genericType, type, converters);
            }

            return converters.ToArray();
        }

        private static void TryAddConverterWith(Type genericType, Type type, List<IConverter> converters)
        {
            try
            {
                var specific = genericType.MakeGenericType(type);
                var instance = (IConverter) Activator.CreateInstance(specific);
                if (instance.IsInitialised)
                {
                    converters.Add(instance);
                }
            }
            catch (Exception ex)
            {
                var asTargetInvocationException = ex as TargetInvocationException;
                // ReSharper disable once RedundantAssignment
                var message = asTargetInvocationException?.Message ?? ex.Message;
                Trace.WriteLine(
                    $"PeanutButter.DuckTyping: Warning: Unable to register automatic string converter for {type}: {message}");
            }
        }

        private static Type[] FindTypesWhichCanTryParseStrings()
        {
            return _typesWhichCanTryParseStrings ??= AllLoadedTypes.Where(HasValidTryParseMethod).ToArray();
        }

        private static Type[] _typesWhichCanTryParseStrings;

        private static bool HasValidTryParseMethod(Type arg)
        {
            return arg.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Any(mi => mi.IsTryParseMethod());
        }

        internal static bool IsTryParseMethod(
            this MethodInfo mi
        )
        {
            if (mi.Name != "TryParse")
            {
                return false;
            }

            var parameters = mi.GetParameters();
            if (parameters.Length != 2)
            {
                return false;
            }

            return parameters[0].ParameterType == typeof(string) &&
                parameters[1].IsOut;
        }

        private static readonly object ConverterLock = new object();

        public static IConverter GetConverter(Type t1, Type t2)
        {
            lock (ConverterLock)
            {
                if (Converters.TryGetValue(Tuple.Create(t1, t2), out var converter))
                {
                    return converter;
                }

                converter = TryMakeValueTypeCastingConverterFor(t1, t2);
                if (converter is null)
                {
                    return null;
                }

                Converters[Tuple.Create(t1, t2)] = converter;
                Converters[Tuple.Create(t2, t1)] = converter;
                return converter;
            }
        }

        private static IConverter TryMakeValueTypeCastingConverterFor(Type t1, Type t2)
        {
            if (!t1.IsValueType || !t2.IsValueType)
            {
                // TODO: look for implicit cast operators?
                return null;
            }

            var defaultOfT1 = t1.DefaultValue();
            var defaultOfT2 = t2.DefaultValue();
            try
            {
                Convert.ChangeType(defaultOfT1, t2);
                Convert.ChangeType(defaultOfT2, t1);
                var converterTypeGeneric = typeof(GenericCastingConverter<,>);
                var converterType = converterTypeGeneric.MakeGenericType(t1, t2);
                // TODO: cache
                return (IConverter) Activator.CreateInstance(converterType);
            }
            catch
            {
                return null;
            }
        }

        private static readonly ConcurrentDictionary<Type, object> DefaultTypeValues = new();

        private static object DefaultValue(this Type type)
        {
            if (DefaultTypeValues.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var method = DefaultValueGenericMethodInfo.MakeGenericMethod(type);
            var result = method.Invoke(null, null);
            DefaultTypeValues.TryAdd(type, result);
            return result;
        }

        private static readonly MethodInfo DefaultValueGenericMethodInfo =
            typeof(ConverterLocator).GetMethod(nameof(DefaultValueGeneric), BindingFlags.Static | BindingFlags.NonPublic);

        private static T DefaultValueGeneric<T>()
        {
            return default(T);
        }


        private static IConverter TryConstruct(Type arg)
        {
            try
            {
                if (arg.IsGenericType)
                    return null;
                return (IConverter) Activator.CreateInstance(arg);
            }
            catch
            {
                return null;
            }
        }

        private static readonly object AllTypesLock = new object();
        private static Type[] _allLoadedTypes;

        private static Type[] AllLoadedTypes
        {
            get
            {
                lock (AllTypesLock)
                {
                    return _allLoadedTypes ??= FindAllLoadedTypes();
                }
            }
        }

        private static Type[] FindAllLoadedTypes()
        {
            var myAsm = typeof(ConverterLocator)
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .Assembly;
            var myTypes = myAsm
                .GetTypes();
            return AppDomain.CurrentDomain.GetAssemblies()
                .Except(new[] { myAsm })
                .Select(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return new Type[0];
                    }
                })
                .SelectMany(a => a)
                // allow satellite assemblies to override
                // converters: first found wins
                .Union(myTypes)
                .ToArray();
        }

        private static Type[] FindConverterTypes()
        {
            var baseType = typeof(IConverter<,>);
            return AllLoadedTypes
                .Where(t => t.GetAllImplementedInterfaces()
                    .Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == baseType))
                .ToArray();
        }

        public static bool HaveConverterFor(Type type, Type toType)
        {
            lock (Converters)
            {
                return Converters.ContainsKey(Tuple.Create(type, toType)) ||
                    Converters.ContainsKey(Tuple.Create(toType, type));
            }
        }

        private static Type[] GetAllImplementedInterfaces(this Type inspectType)
        {
            var result = new List<Type>();
            if (inspectType.IsInterface)
            {
                result.Add(inspectType);
            }

            foreach (var type in inspectType.GetInterfaces())
            {
                result.AddRange(type.GetAllImplementedInterfaces());
            }

            return result.Distinct().ToArray();
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException($"{interfaceType} is not an interface type");
            }

            var interfaces = type.GetInterfaces();
            var nonGenericInterfaces = interfaces.Where(
                i => !i.IsGenericType
            ).ToArray();
            if (nonGenericInterfaces.Contains(interfaceType))
            {
                return true;
            }

            if (!interfaceType.IsGenericType)
            {
                return false;
            }

            var genericInterfaces = interfaces.Where(
                i => i.IsGenericType
            );

            var seeking = interfaceType.GetGenericTypeDefinition();
            var seekingParams = interfaceType.GetGenericArguments();
            return genericInterfaces.Any(
                i =>
                {
                    var genericTypeDef = i.GetGenericTypeDefinition();
                    if (genericTypeDef != seeking)
                    {
                        return false;
                    }

                    var testParams = i.GetGenericArguments();
                    return TypesMatch(testParams, seekingParams);
                }
            );
        }

        private static bool TypesMatch(
            Type[] typeParams,
            Type[] testParams
        )
        {
            if (typeParams.Length == testParams.Length)
            {
                var match = true;
                for (var i = 0; i < typeParams.Length; i++)
                {
                    var testParam = testParams[i];
                    var baseParam = typeParams[i];
                    if (testParam.IsGenericParameter)
                    {
                        var testGenericConstraints = testParam.GetGenericParameterConstraints();
                        if (testGenericConstraints.Any(constraint => !baseParam.Inherits(constraint)))
                        {
                            match = false;
                        }

                        continue;
                    }

                    if (baseParam.IsGenericParameter)
                    {
                        match = false;
                        break;
                    }


                    if (baseParam != testParam)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            if (typeParams.IsEqualTo(testParams))
            {
                return true;
            }

            return false;
        }

        public static bool Inherits(
            this Type type,
            Type test
        )
        {
            if (type is null || test is null)
            {
                return false;
            }

            if (test == typeof(object))
            {
                return true; // everything inherits object
            }

            var baseType = type.BaseType();
            if (baseType is null)
            {
                return false;
            }

            if (baseType.IsGenericType && test.IsGenericType)
            {
                var baseGen = baseType.GetGenericTypeDefinition();
                var testGen = test.GetGenericTypeDefinition();
                if (baseGen == testGen)
                {
                    var baseParams = baseType.GetGenericArguments();
                    var testParams = test.GetGenericArguments();

                    return TypesMatch(baseParams, testParams);
                }
            }


            return baseType == test ||
                baseType.Inherits(test);
        }

        private static bool IsEqualTo<T>(
            this IEnumerable<T> left,
            IEnumerable<T> right
        )
        {
            using var leftEnumerator = left.GetEnumerator();
            using var rightEnumerator = right.GetEnumerator();
            var leftHasValue = leftEnumerator.MoveNext();
            var rightHasValue = rightEnumerator.MoveNext();
            while (leftHasValue && rightHasValue)
            {
                var areEqual = Compare(leftEnumerator.Current, rightEnumerator.Current);
                if (!areEqual)
                {
                    return false;
                }

                leftHasValue = leftEnumerator.MoveNext();
                rightHasValue = rightEnumerator.MoveNext();
            }

            return leftHasValue == rightHasValue;
        }

        private static bool Compare<T1, T2>(
            T1 leftValue,
            T2 rightValue
        )
        {
            if (leftValue is null && rightValue is null)
            {
                return true;
            }

            if (leftValue is null || rightValue is null)
            {
                return false;
            }

            return leftValue.Equals(rightValue);
        }

        internal static Type BaseType(this Type type)
        {
            return type
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .BaseType;
        }
    }
}