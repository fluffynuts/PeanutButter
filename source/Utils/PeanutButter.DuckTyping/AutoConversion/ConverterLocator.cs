using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Imported.PeanutButter.Utils;
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
using Imported.PeanutButter.DuckTyping.AutoConversion.Converters;
using Imported.PeanutButter.DuckTyping.Extensions;
#else
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Extensions;
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
                .Where(converter => converter.Implements(typeof(IConverter<,>)))
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
            temp.ForEach(o =>
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
            });
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
                converters.Add(instance);
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
                .GetAssembly();
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
    }
}