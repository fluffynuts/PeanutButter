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

        public interface IFarmAnimal
        {
            int Legs { get; set; }
        }

        public interface ISingleAnimalFarm
        {
            IFarmAnimal Animal { get; set ; }
        }



        [Test]
        public void GettingPropertyValuesFromNestedMismatchingTypes()
        {
            //--------------- Arrange -------------------
            var toWrap = new
            {
                Animal = new { Legs = 4 }
            };
            var sut = Create(toWrap, typeof(ISingleAnimalFarm), false);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Animal") as IFarmAnimal;

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            // ReSharper disable once PossibleNullReferenceException
            Expect(result.Legs, Is.EqualTo(4));
        }

        public class SomeSingleAnimalFarm
        {
            public object Animal { get; set; }
        }

        public class SomeArbitraryAnimal
        {
            public int Legs { get; set; }
        }

        [Test]
        public void SettingPropertyValuesOnNestedMismatchingTypes()
        {
            //--------------- Arrange -------------------
            var toWrap = new SomeSingleAnimalFarm() { Animal = new SomeArbitraryAnimal() { Legs = 4 } };
            var sut = Create(toWrap, typeof(ISingleAnimalFarm), false);
            var newAnimal = new SomeArbitraryAnimal() { Legs = 100 };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SetPropertyValue("Animal", newAnimal);
            var result = sut.GetPropertyValue("Animal") as IFarmAnimal;

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            // ReSharper disable once PossibleNullReferenceException
            Expect(result.Legs, Is.EqualTo(100));
            result.Legs = 123;
            var newResult = sut.GetPropertyValue("Animal") as IFarmAnimal;
            Expect(newResult.Legs, Is.EqualTo(123));
        }

        public interface IHasGuidId {
            Guid Id { get; }
        }

        [Test]
        public void WhenFuzzy_GetPropertyValue_ShouldBeAbleToConvertFromSource_Guid_ToTarget_String()
        {
            //--------------- Arrange -------------------
            var guid = Guid.NewGuid();
            var toWrap = new {
                Id = guid.ToString()
            };
            var sut = Create(toWrap, typeof(IHasGuidId), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(guid));
        }

        public class WithStringId
        {
            public string id { get; set ; }
        }
        public interface IHasReadWriteGuidId {
            Guid Id { get; set; }
        }

        [Test]
        public void WhenFuzzy_SetPropertyValue_ShouldBeAbleToConvertFromSouce_Guid_ToTarget_String()
        {
            //--------------- Arrange -------------------
            var guid = Guid.NewGuid();
            var toWrap = new WithStringId();
            var sut = Create(toWrap, typeof(IHasReadWriteGuidId), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SetPropertyValue("id", guid);

            //--------------- Assert -----------------------
            Expect(toWrap.id, Is.EqualTo(guid.ToString()));
        }

        public class WithIntId
        {
            public int Id { get; set; }
        }

        public interface IWithIntId
        {
            int Id { get; set; }
        }

        [Test]
        public void SetPropertyValue_WhenUnderlyingFieldIsNotNullable_GivenNull_ShouldSetDefaultValueForFieldType()
        {
            //--------------- Arrange -------------------
            var inner = new WithIntId() { Id = 12 };
            var sut = Create(inner, typeof(IWithIntId), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SetPropertyValue("Id", null);

            //--------------- Assert -----------------------
            Expect(inner.Id, Is.EqualTo(0));
        }

        public interface IWithWriteOnlyId
        {
            int Id { set; }
        }
        public class WithWriteOnlyId
        {
            public int Id { private get; set; }
        }
        [Test]
        public void GetPropertyValue_WhenPropertyIsWriteOnly_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create(new WithWriteOnlyId(), typeof(IWithWriteOnlyId), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.GetPropertyValue("Id"),
                Throws.Exception.InstanceOf<WriteOnlyPropertyException>());

            //--------------- Assert -----------------------
        }

        public interface IWithReadOnlyId
        {
            int Id { get; }
        }
        public class WithReadOnlyId
        {
            public int Id { get; }
        }
        [Test]
        public void SetPropertyValue_WhenPropertyIsReadOnly_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create(new WithReadOnlyId(), typeof(IWithReadOnlyId), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.SetPropertyValue("Id", 1),
                Throws.Exception.InstanceOf<ReadOnlyPropertyException>());

            //--------------- Assert -----------------------
        }

        public class UnderlyingWithNull
        {
            public object Foo { get; set ;}
        }

        public interface IOverlyingNotNullable
        {
            int Foo { get; set; }
        }

        [Test]
        public void GetPropertyValue_WhenUnderlyingValueNotNull_AndPropertyTypeIsNotNullable_AndNoConverter_ShouldReturnDefaultValue()
        {
            //--------------- Arrange -------------------
            var sut = Create(new UnderlyingWithNull() { Foo = null }, typeof(IOverlyingNotNullable), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Foo");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(0));
        }


        [Test]
        public void GetPropertyValue_WhenUnderlyingValueIsNotNull_AndPropertyTypeIsNotNullable_AndNoConverter_ShouldReturnDefaultValue()
        {
            //--------------- Arrange -------------------
            var sut = Create(new UnderlyingWithNull() { Foo = new object() }, typeof(IOverlyingNotNullable), true);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Foo");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(0));
        }

        private ShimSham Create(object toWrap, Type toMimick, bool isFuzzy = false)
        {
            return new ShimSham(toWrap, toMimick, isFuzzy);
        }

        private ShimSham Create(Cow toWrap, bool fuzzy)
        {
            return new ShimSham(toWrap, toWrap.GetType(), fuzzy);
        }
    }
}
