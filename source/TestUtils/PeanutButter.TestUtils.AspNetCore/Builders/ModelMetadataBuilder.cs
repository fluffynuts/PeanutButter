using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;

namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Builds ModelMetadata
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    abstract class ModelMetadataBuilder
{
    /// <summary>
    /// Start a builder for building ModelMetadata for a TModel
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    public static ModelMetadataBuilder<TModel> For<TModel>()
    {
        return new ModelMetadataBuilder<TModel>();
    }

    /// <summary>
    /// Builds the default ModelMetadataBuilder, for an object
    /// </summary>
    /// <returns></returns>
    public static ModelMetadata BuildDefault()
    {
        return new ModelMetadataBuilder<object>().Build();
    }
}

/// <summary>
/// Builds ModelMetadata
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class ModelMetadataBuilder<TModel> : RandomizableBuilder<ModelMetadataBuilder<TModel>, ModelMetadata>
{
    /// <inheritdoc />
    public override ModelMetadataBuilder<TModel> Randomize()
    {
        // TODO
        return this;
    }

    /// <inheritdoc />
    protected override ModelMetadata ConstructEntity()
    {
        return new FakeModelMetadata(ModelMetadataIdentity.ForType(typeof(TModel)));
    }

    /// <summary>
    /// Provide easier access to the faked backing props (all starting with _)
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public ModelMetadataBuilder<TModel> WithFaked(Action<IFakeModelMetadata> mutator)
    {
        return With(o => mutator(o as IFakeModelMetadata));
    }
}