using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.AutoConversion
{
    internal static class ConverterLocator
    {
        internal static IConverter[] Converters =>
            _converters ?? (_converters = FindConverters());

        private static IConverter[] _converters;
        private static IConverter[] FindConverters()
        {
            var converterTypes = FindConverterTypes();
            return converterTypes
                .Select(TryConstruct)
                .Where(c => c != null)
                .Where(IsValidConverterType)
                .Union(MakeStringConverters())
                .ToArray();
        }

        private static bool IsValidConverterType(IConverter converter)
        {
            var genericInterface = converter
                                    .GetType()
                                    .GetAllImplementedInterfaces()
                                    .FirstOrDefault(t => t.IsGenericType &&
                                                t.GetGenericTypeDefinition() == typeof(IConverter<,>));
            if (genericInterface == null)
                return false;
            var types = genericInterface.GetGenericArguments();
            return types.Contains(converter.T1) && types.Contains(converter.T2);
        }

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

        private static void TryAddConverterWith(Type genericType, Type type, List<IConverter> converters)
        {
            try
            {
                var specific = genericType.MakeGenericType(type);
                var instance = (IConverter)Activator.CreateInstance(specific);
                converters.Add(instance);
            }
            catch (Exception ex)
            {
                var asTargetInvocationException = ex as TargetInvocationException;
                // ReSharper disable once RedundantAssignment
                var message = asTargetInvocationException?.Message ?? ex.Message;
                Trace.WriteLine($"PeanutButter.DuckTyping: Warning: Unable to register automatic string converter for {type}: {message}");
            }
        }

        private static Type[] FindTypesWhichCanTryParseStrings()
        {
            return AllLoadedTypes.Where(HasValidTryParseMethod).ToArray();
        }

        private static bool HasValidTryParseMethod(Type arg)
        {
            return arg.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .Any(mi => mi.IsTryParseMethod());
        }


        public static IConverter GetConverter(Type t1, Type t2)
        {
            return Converters.FirstOrDefault(c => CanConvert(c, t1, t2));
        }

        private static bool CanConvert(IConverter converter, Type t1, Type t2)
        {
            var conversionTypes = new[] { converter.T1, converter.T2 };
            return conversionTypes.Contains(t1) && conversionTypes.Contains(t2);
        }

        private static IConverter TryConstruct(Type arg)
        {
            try
            {
                if (arg.IsGenericType)
                    return null;
                return (IConverter)Activator.CreateInstance(arg);
            }
            catch
            {
                return null;
            }
        }

        private static readonly object _allTypesLock = new object();
        private static Type[] _allLoadedTypes;
        private static Type[] AllLoadedTypes
        {
            get
            {
                lock (_allTypesLock)
                {
                    return _allLoadedTypes ?? (_allLoadedTypes = FindAllLoadedTypes());
                }
            }
        }

        private static Type[] FindAllLoadedTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a =>
                        {
                            try
                            {
                                return a.GetTypes();
                            }
                            catch { return new Type[0]; }
                        })
                        .SelectMany(a => a)
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
    }
}