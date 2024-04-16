using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Raised when a StringValueMap changes
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class StringValueMapChangedEventArgs : EventArgs
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