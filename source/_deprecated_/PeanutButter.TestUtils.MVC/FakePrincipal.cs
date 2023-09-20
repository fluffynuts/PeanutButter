using System.Linq;
using System.Security.Principal;

namespace PeanutButter.TestUtils.MVC
{

    public class FakePrincipal : IPrincipal
    {
        public IIdentity Identity { get; }
        private readonly string[] _roles;

        public FakePrincipal(IIdentity identity, string[] roles)
        {
            Identity = identity;
            _roles = roles;
        }

        public bool IsInRole(string role)
        {
            return _roles?.Contains(role) ?? false;
        }
    }



}
