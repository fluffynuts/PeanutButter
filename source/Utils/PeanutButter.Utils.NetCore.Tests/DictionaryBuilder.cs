using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.NetCore.Tests
{
    public class DictionaryBuilder<TKey, TValue>
        : GenericBuilder<DictionaryBuilder<TKey, TValue>, Dictionary<TKey, TValue>>
    {
        public DictionaryBuilder<TKey, TValue> WithItem(TKey key, TValue value)
        {
            return WithProp(o => o.Add(key, value));
        }
    }
}