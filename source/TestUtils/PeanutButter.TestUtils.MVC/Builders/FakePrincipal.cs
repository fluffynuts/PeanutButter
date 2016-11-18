using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace PeanutButter.TestUtils.MVC.Builders
{
    /// <summary>
    /// Original code: http://stephenwalther.com/archive/2008/07/01/asp-net-mvc-tip-12-faking-the-controller-context
    /// </summary>
    public class FakePrincipal : ClaimsPrincipal
    {
        public override IEnumerable<ClaimsIdentity> Identities
            => new[] { new ClaimsIdentity(_identity) };

        public override IIdentity Identity => _identity;
        private readonly IIdentity _identity;
        private readonly string[] _roles;

        public FakePrincipal(IIdentity identity, string[] roles)
        {
            _identity = identity;
            _roles = roles;
        }

        public override bool IsInRole(string role)
        {
            return _roles.Contains(role);
        }
    }



}
