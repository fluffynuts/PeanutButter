using System;

namespace PeanutButter.Utils;

/// <summary>
/// Allows you to rewrite, eg:
/// var foo = o.Prop1 == 1 &amp;&amp;
///           o.Prop2 == 2 &amp;&amp;
///           o.Prop3 == 3 ...
/// as
/// AllOf(
///     o.Prop1 == 1,
///     o.Prop2 == 2,
///     o.Prop3 == 3
/// )
/// (which is easier to read &amp; manipulate, eg in a test,
/// especially if you import with using static)
/// </summary>
public static class Logic
{
    /// <summary>
    /// Returns the result of and'ing all booleans
    /// together
    /// </summary>
    /// <param name="flag1"></param>
    /// <param name="flag2"></param>
    /// <param name="moreFlags"></param>
    /// <returns></returns>
    public static bool AllOf(
        bool flag1,
        bool flag2,
        params bool[] moreFlags
    )
    {
        if (!flag1 || !flag2)
        {
            return false;
        }

        foreach (var f in moreFlags)
        {
            if (!f)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the result of and'ing all booleans (lazy eval)
    /// together
    /// </summary>
    /// <param name="generator1"></param>
    /// <param name="generator2"></param>
    /// <param name="moreGenerators"></param>
    /// <returns></returns>
    public static bool AllOf(
        Func<bool> generator1,
        Func<bool> generator2,
        params Func<bool>[] moreGenerators
    )
    {
        if (!generator1() || !generator2())
        {
            return false;
        }

        foreach (var gen in moreGenerators)
        {
            if (!gen())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the result of or'ing all booleans
    /// together
    /// </summary>
    /// <param name="flag1"></param>
    /// <param name="flag2"></param>
    /// <param name="moreFlags"></param>
    /// <returns></returns>
    public static bool AnyOf(
        bool flag1,
        bool flag2,
        params bool[] moreFlags
    )
    {
        if (flag1 || flag2)
        {
            return true;
        }

        foreach (var f in moreFlags)
        {
            if (f)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the result of or'ing all booleans (lazy eval)
    /// together
    /// </summary>
    /// <param name="generator1"></param>
    /// <param name="generator2"></param>
    /// <param name="moreGenerators"></param>
    /// <returns></returns>
    public static bool AnyOf(
        Func<bool> generator1,
        Func<bool> generator2,
        params Func<bool>[] moreGenerators
    )
    {
        if (generator1() || generator2())
        {
            return true;
        }

        foreach (var gen in moreGenerators)
        {
            if (gen())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true when none of the inputs are true
    /// </summary>
    /// <param name="flag1"></param>
    /// <param name="flag2"></param>
    /// <param name="moreFlags"></param>
    /// <returns></returns>
    public static bool NoneOf(
        bool flag1,
        bool flag2,
        params bool[] moreFlags
    )
    {
        if (flag1 || flag2)
        {
            return false;
        }

        foreach (var f in moreFlags)
        {
            if (f)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true when none of the inputs are true (lazy eval)
    /// </summary>
    /// <param name="generator1"></param>
    /// <param name="generator2"></param>
    /// <param name="moreGenerators"></param>
    /// <returns></returns>
    public static bool NoneOf(
        Func<bool> generator1,
        Func<bool> generator2,
        params Func<bool>[] moreGenerators
    )
    {
        if (generator1() || generator2())
        {
            return false;
        }

        foreach (var gen in moreGenerators)
        {
            if (gen())
            {
                return false;
            }
        }

        return true;
    }
}