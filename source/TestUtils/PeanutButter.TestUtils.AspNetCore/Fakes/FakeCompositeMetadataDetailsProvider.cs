using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Provides a fake ICompositeMetadataDetailsProvider
    /// </summary>
    public class FakeCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
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
}