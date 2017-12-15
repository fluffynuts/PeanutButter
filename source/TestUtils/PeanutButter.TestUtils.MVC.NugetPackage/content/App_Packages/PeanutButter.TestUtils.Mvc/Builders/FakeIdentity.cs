using System;
using System.Security.Principal;

namespace PeanutButter.TestUtils.MVC.Builders
{
    /// <summary>
    /// Original code: http://stephenwalther.com/archive/2008/07/01/asp-net-mvc-tip-12-faking-the-controller-context
    /// </summary>
    public class FakeIdentity : IIdentity
    {
        public FakeIdentity(string userName)
        {
            Name = userName;
        }

        public string AuthenticationType =>
            throw new NotImplementedException();

        public bool IsAuthenticated => !string.IsNullOrEmpty(Name);

        public string Name { get; }
    }
}