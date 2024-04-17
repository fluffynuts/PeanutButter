using System;
using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// Provides Run.Until, a convenience mechanism
/// to run a bit of logic until some condition is
/// met
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static partial class Run
{
    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Until<T>(
        Func<T, bool> validator,
        Func<T> generator
    )
    {
        T result;
        do
        {
            result = generator();
        } while (!validator(result));

        return result;
    }

    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Until<T>(
        Func<T, bool> validator,
        Func<T, T> generator
    )
    {
        T result;
        var last = default(T);
        do
        {
            last = result = generator(last);
        } while (!validator(result));

        return result;
    }

    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Until<T>(
        Func<T, bool> validator,
        Func<T, Task<T>> generator
    )
    {
        T result;
        var last = default(T);
        do
        {
            last = result = await generator(last);
        } while (!validator(result));

        return result;
    }

    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Until<T>(
        Func<T, bool> validator,
        Func<Task<T>> generator
    )
    {
        T result;
        do
        {
            result = await generator();
        } while (!validator(result));

        return result;
    }

    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Until<T>(
        Func<T, Task<bool>> validator,
        Func<T, T> generator
    )
    {
        T result;
        var last = default(T);
        do
        {
            last = result = generator(last);
        } while (!await validator(result));

        return result;
    }

    /// <summary>
    /// Run the generator until the validator
    /// is satisfied and return the result
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="generator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Until<T>(
        Func<T, Task<bool>> validator,
        Func<T, Task<T>> generator
    )
    {
        T result;
        var last = default(T);
        do
        {
            last = result = await generator(last);
        } while (!await validator(result));

        return result;
    }
}