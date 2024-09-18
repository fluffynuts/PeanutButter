using System;
using System.Collections.Concurrent;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators;

/// <summary>
/// Provides the .Randomize extension for objects
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Randomizes properties on the object
    /// - think of it a bit like GetRandom&lt;T&gt;
    ///   but acting on an existing object. In fact,
    ///   where possible, we'll use GetRandom&lt;T&gt;!
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Randomized<T>(this T data) where T : class
    {
        if (data is null)
        {
            return null;
        }

        var type = typeof(T);

        if (
            !GetRandomizableTypeCache.TryGetValue(type, out var canUseGetRandom) ||
            !canUseGetRandom
        )
        {
            try
            {
                var source = RandomValueGen.GetRandom<T>();
                // this is likely to fail if GetRandom<T> returned an NSubstitute sub,
                // with errors about not being able to proceed
                source.CopyPropertiesTo(data);
                GetRandomizableTypeCache[type] = true;
                return data;
            }
            catch
            {
                // exceptions are expensive - don't try this again so that subsequent
                // calls will go a bit faster
                GetRandomizableTypeCache[type] = false;
            }
        }

        return RandomizeProperties(data);
    }

    private static readonly ConcurrentDictionary<Type, bool> GetRandomizableTypeCache = new();

    private static T RandomizeProperties<T>(T data) where T : class
    {
        var props = data.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite)
            {
                continue;
            }

            try
            {
                if (prop.PropertyType.IsClass && prop.CanRead)
                {
                    // try to keep the original references
                    var current = prop.GetValue(data);
                    RandomizeProperties(current);
                }

                prop.SetValue(
                    data,
                    RandomValueGen.GetRandom(prop.PropertyType)
                );
            }
            catch
            {
                // suppress
            }
        }

        return data;
    }

}