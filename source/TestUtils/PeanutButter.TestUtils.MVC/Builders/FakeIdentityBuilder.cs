using PeanutButter.RandomGenerators;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class FakeIdentityBuilder 
        : GenericBuilderWithFieldAccess<FakeIdentityBuilder, FakeIdentity>
    {
        private string _name;

        public FakeIdentityBuilder()
        {
            WithName(GetRandomString(2));
        }

        public FakeIdentityBuilder WithName(string name)
        {
            return WithField(o => o._name = name);
        }

        public override FakeIdentity Build()
        {
            return new FakeIdentity(_name);
        }
    }
}
