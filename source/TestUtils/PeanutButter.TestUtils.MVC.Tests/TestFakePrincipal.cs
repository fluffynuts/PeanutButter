using System.Linq;
using System.Security.Principal;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestFakePrincipal: AssertionHelper
    {
        [Test]
        public void Construct_ShouldStoreParametersOnProperties()
        {
            //--------------- Arrange -------------------
            var identity = Substitute.For<IIdentity>();
            var expectedName = GetRandomString();
            identity.Name.Returns(expectedName);
            var roles = GetRandomCollection<string>(2, 5);


            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new FakePrincipal(identity, roles.ToArray());

            //--------------- Assert -----------------------
            roles.ForEach(r => Expect(sut.IsInRole(r), Is.True));

            Expect(sut.Identity, Is.EqualTo(identity));
            var identities = sut.Identities;
            Expect(identities.Count(), Is.EqualTo(1));
            Expect(identities.Single().Name, Is.EqualTo(expectedName));
        }

    }
}
