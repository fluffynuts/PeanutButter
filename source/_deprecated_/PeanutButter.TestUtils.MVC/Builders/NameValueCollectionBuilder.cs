using System.Collections.Specialized;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class NameValueCollectionBuilder
        : GenericBuilder<NameValueCollectionBuilder, NameValueCollection>
    {
        public NameValueCollectionBuilder WithItem(
            string name,
            string value
        )
        {
            return WithProp(o => o.Add(name, value));
        }
    }
}
