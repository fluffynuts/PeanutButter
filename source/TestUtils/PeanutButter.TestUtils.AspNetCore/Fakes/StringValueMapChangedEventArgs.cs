using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore;

/// <summary>
/// Raised when a StringValueMap changes
/// </summary>
public class StringValueMapChangedEventArgs : EventArgs
{
    /// <summary>
    /// The new values in the map (copy of the map)
    /// </summary>
    public Dictionary<string, StringValues> NewValues { get; }

    /// <inheritdoc />
    public StringValueMapChangedEventArgs(
        IDictionary<string, StringValues> newValues
    )
    {
        // create a copy: event handlers shouldn't be able to modify the original
        NewValues = newValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}