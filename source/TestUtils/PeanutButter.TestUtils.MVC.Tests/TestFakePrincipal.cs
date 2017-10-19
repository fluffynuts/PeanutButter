using System.Linq;
using System.Security.Principal;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestFakePrincipal
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
            roles.ForEach(r => Expect(sut.IsInRole(r)).To.Be.True());

            Expect(sut.Identity).To.Equal(identity);
            var identities = sut.Identities;
            Expect(identities).To.Contain.Only(1)
                .Matched.By(o => o.Name == expectedName);
        }

    }
}
