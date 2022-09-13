using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PeanutButter.TestUtils.AspNetCore.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds a ModelBindingContext for testing
/// </summary>
public class ModelBindingContextBuilder
    : RandomizableBuilder<ModelBindingContextBuilder, ModelBindingContext>
{
    /// <inheritdoc />
    public override ModelBindingContextBuilder Randomize()
    {
        return this;
    }

    /// <summary>
    /// Produces a DefaultModelMetadata instance that is essentially
    /// empty
    /// </summary>
    /// <returns></returns>
    public static DefaultModelMetadata CreateEmptyDefaultModelMetadata()
    {
        var defaultCompositeMetadataDetailsProvider = new DefaultCompositeMetadataDetailsProvider(
            new IMetadataDetailsProvider[0]
        );
        var defaultModelMetadataProvider = new DefaultModelMetadataProvider(
            defaultCompositeMetadataDetailsProvider,
            new DefaultOptions()
        );
        var defaultMetadataDetails = new DefaultMetadataDetails(
            ModelMetadataIdentity.ForType(typeof(object)),
            ModelAttributes.GetAttributesForType(typeof(object))
        );
        return new DefaultModelMetadata(
            defaultModelMetadataProvider, defaultCompositeMetadataDetailsProvider,
            defaultMetadataDetails
        );
    }

    /// <inheritdoc />
    public ModelBindingContextBuilder()
    {
        WithActionContext(ControllerContextBuilder.BuildDefault())
            .WithModelName("Model")
            .WithBindingSource(BindingSource.Body)
            .WithFieldName("Field")
            .WithTopLevelObject(true)
            .WithModel(new { })
            .WithModelMetadata(CreateEmptyDefaultModelMetadata())
            .WithModelState(new ModelStateDictionary())
            .WithValidationState(new ValidationStateDictionary())
            .WithPropertyFilter(_ => true);
    }

    /// <summary>
    /// Sets the ValidationState property
    /// </summary>
    /// <param name="validationState"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithValidationState(
        ValidationStateDictionary validationState
    )
    {
        return With(o => o.ValidationState = validationState);
    }

    /// <summary>
    /// Sets the PropertyFilter property
    /// </summary>
    /// <param name="propertyFilter"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithPropertyFilter(
        Func<ModelMetadata, bool> propertyFilter
    )
    {
        return With(o => o.PropertyFilter = propertyFilter);
    }

    /// <summary>
    /// Sets the ModelState property
    /// </summary>
    /// <param name="modelState"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithModelState(
        ModelStateDictionary modelState
    )
    {
        return With(o => o.ModelState = modelState);
    }

    /// <summary>
    /// Sets the ModelMetadata property
    /// </summary>
    /// <param name="modelMetadata"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithModelMetadata(
        ModelMetadata modelMetadata
    )
    {
        return With(o => o.ModelMetadata = modelMetadata);
    }

    /// <summary>
    /// Sets the Model property
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithModel(
        object model
    )
    {
        return With(o => o.Model = model);
    }

    /// <summary>
    /// Sets the FieldName property
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithFieldName(
        string fieldName
    )
    {
        return With(o => o.FieldName = fieldName);
    }

    /// <summary>
    /// Sets the IsTopLevelObject property
    /// </summary>
    /// <param name="isTopLevelObject"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithTopLevelObject(
        bool isTopLevelObject
    )
    {
        return With(o => o.IsTopLevelObject = isTopLevelObject);
    }

    /// <summary>
    /// Sets the BindingSource property
    /// </summary>
    /// <param name="bindingSource"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithBindingSource(
        BindingSource bindingSource
    )
    {
        return With(o => o.BindingSource = bindingSource);
    }

    /// <summary>
    /// Sets the ModelName property
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithModelName(string modelName)
    {
        return With(o => o.ModelName = modelName);
    }

    /// <summary>
    /// Sets the action context for the model-binding context
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public ModelBindingContextBuilder WithActionContext(
        ActionContext ctx
    )
    {
        return With(
            o => o.ActionContext = ctx
        );
    }

    /// <inheritdoc />
    protected override ModelBindingContext ConstructEntity()
    {
        return new FakeModelBindingContext();
    }
}

/// <summary>
/// Provides a faked implementation of ModelBindingContext
/// </summary>
public class FakeModelBindingContext : ModelBindingContext
{
    /// <inheritdoc />
    public override NestedScope EnterNestedScope(
        ModelMetadata modelMetadata,
        string fieldName,
        string modelName,
        object model
    )
    {
        var ctx = new FakeModelBindingContext();
        TryCopyAllProperties(ctx);
        ctx.IsTopLevelObject = false;
        ctx.ModelName = modelName;
        ctx.FieldName = fieldName;
        ctx.ModelMetadata = modelMetadata;
        ctx.Model = model;
        return new NestedScope(this);
    }

    private void TryCopyAllProperties(
        FakeModelBindingContext ctx
    )
    {
        foreach (var prop in Props)
        {
            try
            {
                prop.SetValue(ctx, prop.GetValue(this));
            }
            catch
            {
                // suppress
            }
        }
    }

    private static readonly PropertyInfo[] Props
        = typeof(ModelBindingContext).GetProperties();

    /// <inheritdoc />
    public override NestedScope EnterNestedScope()
    {
        return new NestedScope();
    }

    /// <inheritdoc />
    protected override void ExitNestedScope()
    {
    }

    /// <inheritdoc />
    public override ActionContext ActionContext { get; set; }

    /// <inheritdoc />
    public override string BinderModelName { get; set; }

    /// <inheritdoc />
    public override BindingSource BindingSource { get; set; }

    /// <inheritdoc />
    public override string FieldName { get; set; }

    /// <inheritdoc />
    public override bool IsTopLevelObject { get; set; }

    /// <inheritdoc />
    public override object Model { get; set; }

    /// <inheritdoc />
    public override ModelMetadata ModelMetadata { get; set; }

    /// <inheritdoc />
    public override string ModelName { get; set; }

    /// <inheritdoc />
    public override ModelStateDictionary ModelState { get; set; }

    /// <inheritdoc />
    public override Func<ModelMetadata, bool> PropertyFilter { get; set; }

    /// <inheritdoc />
    public override ValidationStateDictionary ValidationState { get; set; }

    /// <inheritdoc />
    public override IValueProvider ValueProvider { get; set; }

    /// <inheritdoc />
    public override ModelBindingResult Result { get; set; }
}