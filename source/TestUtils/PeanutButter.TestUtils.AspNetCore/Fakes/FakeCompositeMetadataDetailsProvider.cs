using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a fake ICompositeMetadataDetailsProvider
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
{
    /// <inheritdoc />
    public void CreateBindingMetadata(BindingMetadataProviderContext context)
    {
    }

    /// <inheritdoc />
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
    }

    /// <inheritdoc />
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
    }
}