using System.Linq;
using System.Security.Principal;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.TestUtils.MVC.Builders;
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
        }

    }

    [TestFixture]
    public class TestFakeHttpContextBuilder
    {
        [Test]
        public void ShouldBeAbleToBuildRandom()
        {
            // Arrange
            // Act
            Expect(FakeHttpContextBuilder.BuildRandom)
                .Not.To.Throw();
            // Assert
        }
    }
}
