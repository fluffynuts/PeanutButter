using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Provides a no-op implementation of IObjectModelValidator
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class DefaultObjectModelValidator : IObjectModelValidator
{
    /// <summary>
    /// Does nothing (ie, always assumes valid)
    /// </summary>
    /// <param name="actionContext"></param>
    /// <param name="validationState"></param>
    /// <param name="prefix"></param>
    /// <param name="model"></param>
    public void Validate(
        ActionContext actionContext,
        ValidationStateDictionary validationState,
        string prefix,
        object model
    )
    {
    }
}