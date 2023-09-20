using System.Web.SessionState;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class SessionStateItemCollectionBuilder
        : GenericBuilder<SessionStateItemCollectionBuilder, SessionStateItemCollection>
    {
        public SessionStateItemCollectionBuilder WithItem(
            string name,
            string value
        )
        {
            return WithProp(o => o[name] = value);
        }
    }
}
