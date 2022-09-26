using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <inheritdoc />
    public class FakeTempDataProvider
        : ITempDataProvider
    {
        /// <summary>
        /// The key under which temp data is stored in HttpContext.Items
        /// </summary>
        public const string TEMP_DATA_ITEM_STATE_KEY = "__ControllerTempData";

        /// <inheritdoc />
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            if (context.Items.TryGetValue(TEMP_DATA_ITEM_STATE_KEY, out var result))
            {
                return result as IDictionary<string, object>;
            }
        
            result = new Dictionary<string, object>();
            context.Items[TEMP_DATA_ITEM_STATE_KEY] = result;
            return result as IDictionary<string, object>;
        }

        /// <inheritdoc />
        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            context.Items[TEMP_DATA_ITEM_STATE_KEY] = values;
        }
    }
}