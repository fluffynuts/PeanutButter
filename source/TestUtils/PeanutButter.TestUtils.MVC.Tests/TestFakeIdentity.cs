using System;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestFakeIdentity
    {
        [Test]
        public void Construct_ShouldSetName()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = Create(expected);

            //--------------- Assert -----------------------
            Expect(sut.Name).To.Equal(expected);
        }

        [Test]
        public void IsAuthenticated_WhenNameIsNotEmpty_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var sut = Create(GetRandomString(5));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.IsAuthenticated;

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsAuthenticated_WhenNameIsNullOrEmpty_ShouldReturnTrue(string name)
        {
            //--------------- Arrange -------------------
            var sut = Create(name);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.IsAuthenticated;

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void AuthenticationType_ShouldThrowNotImplemented()
        {
            //--------------- Arrange -------------------
            var sut = Create(GetRandomString());

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.AuthenticationType)
                .To.Throw<NotImplementedException>();

            //--------------- Assert -----------------------
        }

        private FakeIdentity Create(string name)
        {
            return new FakeIdentity(name);
        }
    }
}