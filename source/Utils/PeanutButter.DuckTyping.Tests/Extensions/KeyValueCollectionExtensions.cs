using System.Collections.Generic;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    public static class KeyValueCollectionExtensions
    {
        public static KeyValuePair<string, object> AsKeyValuePairOfStringObject(this KeyValuePair<string, string> src)
        {
            return new KeyValuePair<string, object>(
                src.Key,
                src.Value
            );
        }
    }
}