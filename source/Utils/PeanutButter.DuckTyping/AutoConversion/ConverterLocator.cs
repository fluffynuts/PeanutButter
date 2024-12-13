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
namespace Imported.PeanutButter.DuckTyping.AutoConversion;
#else
namespace PeanutButter.DuckTyping.AutoConversion;
#endif
/// <summary>
/// Locates converters for converting values from one type to another
/// </summary>
public static class ConverterLocator
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
            .Select(
                converter =>
                {
                    // what if a converter implements multiple IConverter<,> interfaces?
                    try
                    {
                        var iface = converter.GetType().GetInterfaces()
                            .Single(
                                i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IConverter<,>)
                            );
                        var types = iface.GetGenericArguments();
                        return new[]
                        {
                            new
                            {
                                key = Tuple.Create(types[0], types[1]),
                                value = converter
                            },
                            new
                            {
                                key = Tuple.Create(types[1], types[0]),
                                value = converter
                            }
                        };
                    }
                    catch
                    {
                        return null;
                    }
                }
            )
            .SelectMany(o => o)
            .Where(o => o is not null);

        var result = new Dictionary<Tuple<Type, Type>, IConverter>();
        temp.ForEach(
            o =>
            {
                if (!result.TryAdd(o.key, o.value))
                {
                    Trace.WriteLine(
                        $"WARNING: Converter {
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
        );
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
        }.Union(NumericTypes).Select(
            type =>
            {
                var converterType = genericType.MakeGenericType(type);
                return (IConverter)Activator.CreateInstance(converterType);
            }
        ).ToArray();
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

    private static void TryAddConverterWith(
        Type genericType,
        Type type,
        List<IConverter> converters
    )
    {
        try
        {
            var instance = CreateStringConverterFromGenericBase(genericType, type);
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
                $"PeanutButter.DuckTyping: Warning: Unable to register automatic string converter for {type}: {message}"
            );
        }
    }

    private static IConverter CreateStringConverterFromGenericBase(
        Type genericConverterType,
        Type typeToConvert
    )
    {
        var specific = genericConverterType.MakeGenericType(typeToConvert);
        var instance = Activator.CreateInstance(specific) as IConverter
            ?? throw new InvalidOperationException(
                $"{specific} does not implement IConverter"
            );
        return instance;
    }

    private static Type[] FindTypesWhichCanTryParseStrings()
    {
        return _typesWhichCanTryParseStrings ??= AllLoadedTypes.Where(
            HasValidTryParseMethod
        ).ToArray();
    }

    private static Type[] _typesWhichCanTryParseStrings;

    private static bool HasValidTryParseMethod(Type arg)
    {
        return arg.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Any(mi => mi.IsTryParseMethod());
    }

    private static readonly object ConverterLock = new();

    /// <summary>
    /// Attempts to locate a converter to convert between
    /// types t1 and t2
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static IConverter TryFindConverter(Type t1, Type t2)
    {
        lock (ConverterLock)
        {
            if (Converters.TryGetValue(Tuple.Create(t1, t2), out var converter))
            {
                return converter;
            }

            converter = TryMakeEnumStringConverterFor(t1, t2)
                ?? TryMakeValueTypeCastingConverterFor(t1, t2);

            if (converter is null)
            {
                return null;
            }

            Converters[Tuple.Create(t1, t2)] = converter;
            Converters[Tuple.Create(t2, t1)] = converter;
            return converter;
        }
    }

    private static IConverter TryMakeEnumStringConverterFor(
        Type t1,
        Type t2
    )
    {
        var all = new[]
        {
            t1,
            t2
        };
        var stringType = all.FirstOrDefault(o => o == typeof(string));
        if (stringType is null)
        {
            return null;
        }

        var enumType = all.FirstOrDefault(o => o.IsEnum);
        var genericType = GenericStringToEnumConverterType;
        if (enumType is null)
        {
            enumType = all.Select(Nullable.GetUnderlyingType)
                .FirstOrDefault(o => o is not null && o.IsEnum);
            if (enumType is null)
            {
                return null;
            }
            genericType = GenericStringToNullableEnumConverterType;
        }

        return CreateStringConverterFromGenericBase(
            genericType,
            enumType
        );
    }

    private static readonly Type GenericStringToEnumConverterType =
        typeof(GenericStringToEnumConverter<>);

    private static readonly Type GenericStringToNullableEnumConverterType =
        typeof(GenericStringToNullableEnumConverter<>);

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
            _ = Convert.ChangeType(defaultOfT1, t2);
            _ = Convert.ChangeType(defaultOfT2, t1);

            var converterTypeGeneric = typeof(GenericCastingConverter<,>);
            var converterType = converterTypeGeneric.MakeGenericType(t1, t2);
            // TODO: cache
            return (IConverter)Activator.CreateInstance(converterType);
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
            {
                return null;
            }

            return (IConverter)Activator.CreateInstance(arg);
        }
        catch
        {
            return null;
        }
    }

    private static readonly object AllTypesLock = new();
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
            .Except(
                new[]
                {
                    myAsm
                }
            )
            .Select(
                a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return new Type[0];
                    }
                }
            )
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
            .Where(
                t => t.GetAllImplementedInterfaces()
                    .Any(
                        i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == baseType
                    )
            )
            .ToArray();
    }

    /// <summary>
    /// Determines if there is a converter to convert
    /// between the two provided types
    /// </summary>
    /// <param name="type"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static bool HaveConverterFor(Type type, Type toType)
    {
        lock (Converters)
        {
            return Converters.ContainsKey(Tuple.Create(type, toType)) ||
                Converters.ContainsKey(Tuple.Create(toType, type));
        }
    }
}