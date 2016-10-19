using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestShimSham: AssertionHelper
    {
        // Most of ShimSham is tested indirectly throug tests on TypeMaker
        //  However I'd like to start testing more from basics (:

        public class Cow
        {
            public string LastPitch { get; set; }
            public int LastCount { get; set; }
            public void Moo(int howManyTimes, string withPitch)
            {
                LastCount = howManyTimes;
                LastPitch = withPitch;
            }
        }
        [Test]
        public void CallThrough_GivenParameterOrderMisMatch_WhenConstructedWithFlimFlamFalse_ShouldExplode()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var sut = Create(toWrap, false);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.CallThrough("Moo", new object[] { "high-pitched", 2 }),
                Throws.Exception.InstanceOf<ArgumentException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void CallThrough_GivenParameterOrderMismatch_WhenConstructedWithFlimFlamTrue_ShouldPerformTheCall()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var sut = Create(toWrap, true);
            var expectedPitch = GetRandomString();
            var expectedCount = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.CallThrough("Moo", new object[] { expectedPitch, expectedCount } ),
                Throws.Nothing);

            //--------------- Assert -----------------------
            Expect(toWrap.LastPitch, Is.EqualTo(expectedPitch));
            Expect(toWrap.LastCount, Is.EqualTo(expectedCount));
        }


        [Test]
        public void GetPropertyValue_WhenConstructedWith_FuzzyFalse_AndPropertyCaseMisMatch_ShouldFail()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            toWrap.LastPitch = GetRandomString();
            var sut = Create(toWrap, false);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.GetPropertyValue("lastPitch"),
                Throws.Exception.InstanceOf<PropertyNotFoundException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void GetPropertyValue_WhenConstructedWith_FuzzyTrue_AndPropertyCaseMismatch_ShouldReturnPropertyValue()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var expected = GetRandomString();
            toWrap.LastPitch = expected;
            var sut = Create(toWrap, true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("lastPitch");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetPropertyValue_WhenConstructedWith_FuzzyTrue_AndPropertyCaseMismatch_ShouldSetProperty()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var expected = GetRandomString();
            var sut = Create(toWrap, true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SetPropertyValue("LasTpItch", expected);

            //--------------- Assert -----------------------
            Expect(toWrap.LastPitch, Is.EqualTo(expected));
        }

        [Test]
        public void SetPropertyValue_WhenConstructedWith_FuzzyFalse_AndPropertyCaseMismatch_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var expected = GetRandomString();
            var sut = Create(toWrap, false);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.SetPropertyValue("LasTpItch", expected),
                Throws.Exception.InstanceOf<PropertyNotFoundException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void CallThrough_WhenConstructedWith_FuzzyFalse_AndMethodNameCaseMismatch_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var pitch = GetRandomString();
            var count = GetRandomInt();
            var sut = Create(toWrap, false);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.CallThrough("mOo", new object[] { count, pitch }),
                Throws.Exception.InstanceOf<MethodNotFoundException>());

            //--------------- Assert -----------------------

        }


        [Test]
        public void CallThrough_WhenConstructedWith_FuzzyTrue_AndMethodNameCaseMismatch_ShoulCallThrough()
        {
            //--------------- Arrange -------------------
            var toWrap = new Cow();
            var pitch = GetRandomString();
            var count = GetRandomInt();
            var sut = Create(toWrap, true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.CallThrough("mOo", new object[] { count, pitch });

            //--------------- Assert -----------------------
            Expect(toWrap.LastCount, Is.EqualTo(count));
            Expect(toWrap.LastPitch, Is.EqualTo(pitch));

        }






        private ShimSham Create(Cow toWrap, bool fuzzy)
        {
            return new ShimSham(toWrap, fuzzy);
        }
    }
}
