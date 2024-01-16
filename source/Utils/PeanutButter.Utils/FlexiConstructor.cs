using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// (optional) flags to pass to the Construct&lt;T&gt; method
/// </summary>
public enum ConstructFlags
{
    /// <summary>
    /// No flags
    /// </summary>
    None = 0,

    /// <summary>
    /// Provided values must match the exact types for the constructor args
    /// </summary>
    MatchTypesExactly,

    /// <summary>
    /// Throw when one or more provided constructor arguments cannot be used
    /// </summary>
    ErrorWhenUnusedArguments,

    /// <summary>
    /// Throw when one or more constructor parameters was not specified
    /// </summary>
    ErrorWhenUnspecifiedParameters
}

/// <summary>
/// Provides a static method to construct
/// other entities with a more flexible
/// parameter ordering:
/// - parameters are fed in the order of
///   "first with matching type"
/// - missing parameters are fed in as
///     the default value for the type
/// </summary>
public class FlexiConstructor
{
    /// <summary>
    /// Attempts to construct the type T with the provided
    /// constructor parameters, re-ordering as necessary
    /// - will provide default values for missing parameters
    /// - will attempt basic type changes to satisfy the constructor
    /// </summary>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Construct<T>(params object[] args)
    {
        return Construct<T>(ConstructFlags.None, args);
    }

    /// <summary>
    /// Attempts to construct the type T with the provided
    /// constructor parameters, re-ordering as necessary
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Construct<T>(
        ConstructFlags flags,
        params object[] args
    )
    {
        return Construct<T>(
            t => t.DefaultValue(),
            flags,
            args
        );
    }

    /// <summary>
    /// Attempts to construct the type T with the provided
    /// constructor parameters, re-ordering as necessary. The
    /// provided fallbackFactory is used for missing dependencies
    /// instead of simply providing the default value.
    /// </summary>
    /// <param name="fallbackFactory"></param>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Construct<T>(
        Func<Type, object> fallbackFactory,
        params object[] args
    )
    {
        return Construct<T>(
            fallbackFactory,
            ConstructFlags.None,
            args
        );
    }

    /// <summary>
    /// Attempts to construct the type T with the provided
    /// constructor parameters, re-ordering as necessary. The
    /// provided fallbackFactory is used for missing dependencies
    /// instead of simply providing the default value.
    /// </summary>
    /// <param name="fallbackFactory"></param>
    /// <param name="flags"></param>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Construct<T>(
        Func<Type, object> fallbackFactory,
        ConstructFlags flags,
        params object[] args
    )
    {
        var constructors = ConstructorCache
            .FindOrAdd(
                typeof(T),
                () => typeof(T).GetConstructors()
            );
        var selectedParameters = FindBestMatch(fallbackFactory, flags, constructors, args);
        if (selectedParameters is null)
        {
            throw new InvalidOperationException(
                "Unable to find matching constructor for provided parameters"
            );
        }

        return (T)Activator.CreateInstance(typeof(T), selectedParameters);
    }

    private static object[] FindBestMatch(
        Func<Type, object> fallbackFactory,
        ConstructFlags flags,
        ConstructorInfo[] constructorInfos,
        object[] args
    )
    {
        var scored = new List<ScoredConstructor>();
        foreach (var ctor in constructorInfos)
        {
            var remainingArgs = new List<object>(args);
            var mapped = ctor.GetParameters()
                .Select(
                    pi =>
                    {
                        if (remainingArgs.TryEjectFirst(
                                o => o is not null && o.GetType() == pi.ParameterType,
                                out var value
                            ))
                        {
                            return new ConstructorParameter(
                                pi.ParameterType,
                                value,
                                value,
                                ParameterFlags.WasProvided | ParameterFlags.IsExactTypeMatch
                            );
                        }

                        if (remainingArgs.TryEjectFirst(
                                o => o is not null && pi.ParameterType.IsInstanceOfType(o),
                                out value
                            ))
                        {
                            return new ConstructorParameter(
                                pi.ParameterType,
                                value,
                                value,
                                ParameterFlags.WasProvided
                            );
                        }
                        // TODO: clever casting?

                        var val = fallbackFactory(pi.ParameterType);
                        return new ConstructorParameter(
                            pi.ParameterType,
                            val,
                            val,
                            ParameterFlags.None
                        );
                    }
                ).ToArray();
            scored.Add(new ScoredConstructor(ctor, mapped, args));
        }

        var bestMatch = scored
            .OrderBy(o => o.Score)
            .FirstOrDefault();
        if (bestMatch is null)
        {
            return null;
        }

        bestMatch.Validate(flags);

        return bestMatch
            .Parameters
            ?.Select(o => o.Value)
            .ToArray();
    }

    private class ScoredConstructor
    {
        public ConstructorInfo Info { get; }
        public ConstructorParameter[] Parameters { get; }
        public object[] OriginalParameters { get; }
        public decimal Score => _score = CalculateScore();
        private decimal _score;

        private decimal CalculateScore()
        {
            return Parameters.Length +
                2 * PercentageOfExactMatches() +
                PercentageOfForcedMatches();
        }

        public ScoredConstructor(
            ConstructorInfo info,
            ConstructorParameter[] parameters,
            object[] originalParameters
        )
        {
            Info = info;
            Parameters = parameters;
            OriginalParameters = originalParameters;
        }

        private decimal PercentageOfExactMatches()
        {
            if (Parameters.Length == 0)
            {
                return 0;
            }

            var exactMatches = (decimal)Parameters.Count(
                o => o.Flags.HasFlag(ParameterFlags.WasProvided) &&
                    o.Flags.HasFlag(ParameterFlags.IsExactTypeMatch)
            );
            return exactMatches / (decimal)Parameters.Length;
        }

        private decimal PercentageOfForcedMatches()
        {
            if (Parameters.Length == 0)
            {
                return 0;
            }

            var exactMatches = (decimal)Parameters.Count(
                o => o.Flags.HasFlag(ParameterFlags.WasProvided) &&
                    !o.Flags.HasFlag(ParameterFlags.IsExactTypeMatch)
            );
            return exactMatches / (decimal)Parameters.Length;
        }

        public void Validate(ConstructFlags flags)
        {
            if (flags == ConstructFlags.None)
            {
                return;
            }

            if (flags.HasFlag(ConstructFlags.ErrorWhenUnspecifiedParameters))
            {
                VerifyNoUnspecifiedParameters();
            }

            if (flags.HasFlag(ConstructFlags.MatchTypesExactly))
            {
                ValidateTypesAreExactMatch();
            }

            if (flags.HasFlag(ConstructFlags.ErrorWhenUnusedArguments))
            {
                ValidateNoUnusedParameters();
            }
        }

        private void ValidateNoUnusedParameters()
        {
            var mapped = Parameters.Select(o => o.MappedFrom).ToArray();
            var missing = OriginalParameters.Except(mapped)
                .ToArray();
            if (missing.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                missing.AsTextListWithHeader("One or more specified parameters was not used")
            );
        }

        private void VerifyNoUnspecifiedParameters()
        {
            var missing = Parameters
                .Where(o => !o.Flags.HasFlag(ParameterFlags.WasProvided))
                .ToArray();
            if (missing.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                missing.AsTextList(
                    "One or more constructor parameters were not specified"
                )
            );
        }


        private void ValidateTypesAreExactMatch()
        {
            var mismatched = Parameters.Where(
                o => !o.Flags.HasFlag(ParameterFlags.IsExactTypeMatch)
            ).ToArray();
            if (mismatched.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $@"{
                    mismatched
                        .Select(o => $"{o.Value?.GetType()} for parameter of type {o.ParameterType}")
                        .AsTextListWithHeader("One or more parameters was provided with the incorrect type and strict type matching was enabled")
                }"
            );
        }
    }

    [Flags]
    private enum ParameterFlags
    {
        None = 0,
        WasProvided = 1,
        IsExactTypeMatch = 2
    }

    private class ConstructorParameter
    {
        public Type ParameterType { get; }
        public object Value { get; }
        public ParameterFlags Flags { get; }
        public object MappedFrom { get; }

        public ConstructorParameter(
            Type parameterType,
            object mappedFrom,
            object value,
            ParameterFlags flags
        )
        {
            ParameterType = parameterType;
            MappedFrom = mappedFrom;
            Value = value; // ConvertIfRequired(parameterType, value);
            Flags = flags;
        }

        // private object ConvertIfRequired(
        //     Type requiredType,
        //     object value
        // )
        // {
        //     if (value is null)
        //     {
        //         return requiredType.DefaultValue();
        //     }
        //     var vt = value.GetType();
        //     if (requiredType.IsAssignableFrom(vt))
        //     {
        //         return value;
        //     }
        //
        //     var convert = Converters.TryGetValue(requiredType, out var c)
        //         ? c
        //         : ChangeType;
        //
        //     try
        //     {
        //         return convert(value, requiredType);
        //     }
        //     catch
        //     {
        //         return value;
        //     }
        // }
        //
        // private static object ChangeType(object value, Type required)
        // {
        //     return Convert.ChangeType(value, required);
        // }
        //
        // private static readonly Dictionary<Type, Func<object, Type, object>> Converters = new()
        //     {
        //         [typeof(decimal)] = ConvertViaDecimalDecorator,
        //         [typeof(double)] = ConvertViaDecimalDecorator,
        //         [typeof(float)] = ConvertViaDecimalDecorator
        //     };
        //
        // private static object ConvertViaDecimalDecorator(
        //     object value,
        //     Type t
        // )
        // {
        //     var dd = new DecimalDecorator($"{value}");
        //     return Convert.ChangeType(dd.ToDecimal(), t);
        // }
    }

    private static readonly ConcurrentDictionary<Type, ConstructorInfo[]>
        ConstructorCache = new();
}