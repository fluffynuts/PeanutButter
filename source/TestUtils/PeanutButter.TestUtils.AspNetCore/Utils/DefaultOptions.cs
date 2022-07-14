using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Provides a null-implementation for IOptions&lt;MvcOptions&gt;
/// </summary>
public class DefaultOptions : IOptions<MvcOptions>
{
    /// <inheritdoc />
    public MvcOptions Value { get; }

    /// <summary>
    /// 
    /// </summary>
    public DefaultOptions()
    {
        Value = new MvcOptions();
    }
}