using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Build a ViewDataDictionary
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class ViewDataDictionaryBuilder
    : Builder<ViewDataDictionaryBuilder, ViewDataDictionary>
{
    /// <inheritdoc />
    public ViewDataDictionaryBuilder()
    {
        WithModel(new object());
    }

    /// <summary>
    /// Set the model on the ViewDataDictionary
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public ViewDataDictionaryBuilder WithModel(object model)
    {
        return With(o => o.Model = model);
    }

    /// <summary>
    /// Add arbitrary data to the view data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ViewDataDictionaryBuilder With(
        string key,
        object value
    )
    {
        return With(o => o[key] = value);
    }

    /// <inheritdoc />
    protected override ViewDataDictionary ConstructEntity()
    {
        return new ViewDataDictionary(
            ModelMetadataBuilder.BuildDefault(),
            new ModelStateDictionary()
        );
    }
}

/// <summary>
/// Implementation of IOptions&lt;MvcOptions&gt; to be used
/// for generating ViewDataDictionaries, or whatever else
/// you might like (:
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class OptionsAccessor : IOptions<MvcOptions>
{
    /// <inheritdoc />
    public MvcOptions Value { get; } = new MvcOptions();
}