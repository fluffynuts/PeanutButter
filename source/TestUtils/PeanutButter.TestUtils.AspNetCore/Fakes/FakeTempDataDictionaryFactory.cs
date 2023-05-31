using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <inheritdoc />
    public class FakeTempDataDictionaryFactory : ITempDataDictionaryFactory
    {
        private static readonly object Key = typeof(ITempDataDictionary);

        /// <inheritdoc />
        public ITempDataDictionary GetTempData(HttpContext context)
        {
            if (
                context.Items.TryGetValue(Key, out var result) &&
                result is not null
            )
            {
                return (ITempDataDictionary) result;
            }

            result = new TempDataDictionary(context, new FakeTempDataProvider());
            context.Items[Key] = result;
            return (ITempDataDictionary) result;
        }
    }
}