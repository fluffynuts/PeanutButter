using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Build a ViewDataDictionary
    /// </summary>
    public class ViewDataDictionaryBuilder
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

        /// <inheritdoc />
        protected override ViewDataDictionary ConstructEntity()
        {
            var compositeProvider = new DefaultCompositeMetadataDetailsProvider(
                new[] { new DefaultBindingMetadataProvider() }
            );
            var optionsAccessor = new OptionsAccessor();
            var provider = new DefaultModelMetadataProvider(
                compositeProvider,
                optionsAccessor
            );
            return new ViewDataDictionary(
                provider,
                new ModelStateDictionary()
            );

        }
        
        /// <summary>
        /// Implementation of IOptions&lt;MvcOptions&gt; to be used
        /// for generating ViewDataDictionaries, or whatever else
        /// you might like (:
        /// </summary>
        public class OptionsAccessor : IOptions<MvcOptions>
        {
            /// <inheritdoc />
            public MvcOptions Value { get; } = new MvcOptions();
        }

    }
}