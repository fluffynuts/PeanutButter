using System.Collections.Generic;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer;

/// <summary>
/// Mimics asp.net's StringValues without pulling in
/// the dependency: stores one or more strings
/// </summary>
public class Strings
{
    /// <summary>
    /// All values stores in this instance
    /// </summary>
    public readonly List<string> Values = new();

    internal Strings(string value)
    {
        Values.Add(value);
    }

    internal Strings(IEnumerable<string> values)
    {
        Values.AddRange(values);
    }

    internal void AddValue(string value)
    {
        Values.Add(value);
    }

    /// <summary>
    /// Implicitly convert this instance to a string
    /// </summary>
    /// <param name="strings"></param>
    /// <returns></returns>
    public static implicit operator string(Strings strings)
    {
        return strings.ToString();
    }

    /// <summary>
    /// Implicitly convert a string to an instance of StringValues
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Strings(string value)
    {
        return new(value);
    }

    /// <summary>
    /// Overriding equality
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is string str)
        {
            return str == ToString();
        }

        if (obj is Strings sv)
        {
            if (sv.Values.Count != Values.Count)
            {
                return false;
            }

            for (var i = 0; i < sv.Values.Count; i++)
            {
                if (sv.Values[i] != Values[i])
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Override GetHashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    /// <summary>
    /// Render all the values, comma-separated
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Values.JoinWith(",");
    }
}