using System;
using System.Collections.Generic;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.RandomGenerators;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestDictionaryShimSham: AssertionHelper
    {
        [Test]
        public void Construct_GivenNullDictionary_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var input = null as Dictionary<string, object>;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => Create(input),
                Throws.Nothing);

            //--------------- Assert -----------------------
        }

        [Test]
        public void GetPropertyValue_WhenDataIsNull_ShouldThrow_PropertyNotFoundException()
        {
            //--------------- Arrange -------------------
            var sut = Create(null);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.GetPropertyValue(RandomValueGen.GetRandomString()),
                Throws.Exception.InstanceOf<PropertyNotFoundException>());

            //--------------- Assert -----------------------
        }

        public interface IHaveId
        {
            int Id { get; set; }
        }

        [Test]
        public void GetPropertyValue_WhenMimickedInterfaceDoesNotContainMentionedProperty_ShouldThrowPropertyNotFoundException()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var type = typeof(IHaveId);
            var sut = Create(data, type);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.GetPropertyValue(RandomValueGen.GetRandomString(10, 20)),
                Throws.Exception.InstanceOf<PropertyNotFoundException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void GetPropertyValue_WhenMimickedInterfaceContainsPropertyANdDictionaryDoesNot_ShouldReturnDefaultValue()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var type = typeof(IHaveId);
            var sut = Create(data, type);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(default(int)));
        }



        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMatchesType_ShouldReturnValue()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var data = new Dictionary<string, object>()
            {
                { "Id", expected }
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMatchesTypeAndDictionaryIsInsensitive_ShouldReturnValue()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", expected }
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("iD");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMisMatchesTypeAndCantConvert_ShouldReturnDefault()
        {
            //--------------- Arrange -------------------
            var expected = default(int);
            var data = new Dictionary<string, object>()
            {
                { "Id", RandomValueGen.GetRandomAlphaString() }
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetPropertyValue_GivenUnkownPropertyName_ShouldThrow_PropertyNotFoundException()
        {
            //--------------- Arrange -------------------
            var sut = Create(null, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.SetPropertyValue(RandomValueGen.GetRandomString(10, 20), RandomValueGen.GetRandomInt()),
                Throws.Exception.InstanceOf<PropertyNotFoundException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void SetPropertyValue_GivenKnownPropertyNameAndValidValue_ShouldSetProperty()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var expected = RandomValueGen.GetRandomInt();
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                sut.SetPropertyValue("Id", expected.ToString()),
                Throws.Nothing);

            //--------------- Assert -----------------------
            Expect(data["Id"], Is.EqualTo(expected));
        }

        [Test]
        public void SetPropertyValue_GivenKnownPropertyNameAndConvertableValue_ShouldSetPropertyWithCorrectType()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var expected = RandomValueGen.GetRandomInt();
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SetPropertyValue("Id", expected.ToString());

            //--------------- Assert -----------------------
            Expect(data["Id"], Is.EqualTo(expected));
        }

        public interface INested
        {
            IHaveId HaveId { get; set; }
        }

        [Test]
        public void GetPropertyValue_WhenHaveSubInterface_ShouldShim()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var data = new Dictionary<string, object>()
            {
                { "HaveId", new Dictionary<string, object>() { { "Id", expected }} }
            };
            var sut = Create(data, typeof(INested));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("HaveId");

            //--------------- Assert -----------------------
            var castResult = result as IHaveId;
            Expect(castResult, Is.Not.Null);
            Expect(castResult.Id, Is.EqualTo(expected));
        }

        [Test]
        public void SetPropertyValue_WhenHaveSubInterface_ShouldSetOnShimThroughToOriginal()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var original = RandomValueGen.GetAnother(expected);
            var data = new Dictionary<string, object>()
            {
                { "HaveId", new Dictionary<string, object>() { { "Id", original } } }
            };
            var sut = Create(data, typeof(INested));
            var sub = sut.GetPropertyValue("HaveId") as IHaveId;

            //--------------- Assume ----------------
            Expect(sub, Is.Not.Null);

            //--------------- Act ----------------------
            sub.Id = expected;

            //--------------- Assert -----------------------
            Expect((data["HaveId"] as Dictionary<string, object>)["Id"], Is.EqualTo(expected));
        }





        public interface IEmpty
        {
        }

        private IShimSham Create(
            Dictionary<string, object> data,
            Type interfaceToMimick = null)
        {
            return new DictionaryShimSham(
                data,
                interfaceToMimick ?? typeof(IEmpty)
            );
        }

    }
}