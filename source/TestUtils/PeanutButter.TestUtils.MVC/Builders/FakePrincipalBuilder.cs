using System.Collections.Generic;
using System.Security.Principal;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class FakePrincipalBuilder : GenericBuilderWithFieldAccess<FakePrincipalBuilder, FakePrincipal>
    {
        private IIdentity _identity;
        private readonly List<string> _roles = new List<string>();

        public override FakePrincipal ConstructEntity()
        {
            return new FakePrincipal(
                _identity, 
                _roles.ToArray());
        }

        public FakePrincipalBuilder()
        {
            WithIdentity(FakeIdentityBuilder.BuildDefault());
        }

        public FakePrincipalBuilder WithIdentity(IIdentity identity)
        {
            return WithField(b => b._identity = identity);
        }

        public FakePrincipalBuilder WithRoles(params string[] roles)
        {
            return WithField(b => b._roles.AddRange(roles));
        }
    }
}
