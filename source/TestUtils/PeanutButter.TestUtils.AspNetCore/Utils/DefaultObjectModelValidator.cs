using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Provides a no-op implementation of IObjectModelValidator
/// </summary>
public class DefaultObjectModelValidator : IObjectModelValidator
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