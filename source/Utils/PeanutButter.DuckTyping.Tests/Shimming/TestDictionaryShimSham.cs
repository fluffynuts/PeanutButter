using System;
using System.Collections.Generic;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.RandomGenerators;

// ReSharper disable UnusedMember.Global
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestDictionaryShimSham
    {
        [Test]
        public void Construct_GivenNullDictionary_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var input = null as Dictionary<string, object>;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => Create(input)).Not.To.Throw();

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
                    sut.GetPropertyValue(RandomValueGen.GetRandomString()))
                .To.Throw<PropertyNotFoundException>();

            //--------------- Assert -----------------------
        }

        public interface IHaveId
        {
            int Id { get; set; }
        }

        [Test]
        public void
            GetPropertyValue_WhenMimickedInterfaceDoesNotContainMentionedProperty_ShouldThrowPropertyNotFoundException()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var type = typeof(IHaveId);
            var sut = Create(data, type);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.GetPropertyValue(RandomValueGen.GetRandomString(10, 20)))
                .To.Throw<PropertyNotFoundException>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void
            GetPropertyValue_WhenMimickedInterfaceContainsPropertyANdDictionaryDoesNot_ShouldReturnDefaultValue()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>();
            var type = typeof(IHaveId);
            var sut = Create(data, type);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result).To.Equal(default(int));
        }


        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMatchesType_ShouldReturnValue()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var data = new Dictionary<string, object>()
            {
                {"Id", expected}
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMatchesTypeAndDictionaryIsInsensitive_ShouldReturnValue()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {"id", expected}
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("iD");

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetPropertyValue_WhenPropertyDataExistsAndMisMatchesTypeAndCantConvert_ShouldReturnDefault()
        {
            //--------------- Arrange -------------------
            var expected = default(int);
            var data = new Dictionary<string, object>()
            {
                {"Id", RandomValueGen.GetRandomAlphaString()}
            };
            var sut = Create(data, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("Id");

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void SetPropertyValue_GivenUnkownPropertyName_ShouldThrow_PropertyNotFoundException()
        {
            //--------------- Arrange -------------------
            var sut = Create(null, typeof(IHaveId));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() =>
                    sut.SetPropertyValue(RandomValueGen.GetRandomString(10, 20), RandomValueGen.GetRandomInt())
                )
                .To.Throw<PropertyNotFoundException>();

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
                    sut.SetPropertyValue("Id", expected.ToString()))
                    .Not.To.Throw();

            //--------------- Assert -----------------------
            Expect(data)
                .To.Contain.Key("Id")
                .With.Value(expected);
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
            Expect(data)
                .To.Contain.Key("Id")
                .With.Value(expected);
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
                {"HaveId", new Dictionary<string, object>() {{"Id", expected}}}
            };
            var sut = Create(data, typeof(INested));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertyValue("HaveId");

            //--------------- Assert -----------------------
            var castResult = result as IHaveId;
            Expect(castResult).Not.To.Be.Null();
            Expect(castResult.Id).To.Equal(expected);
        }

        [Test]
        public void SetPropertyValue_WhenHaveSubInterface_ShouldSetOnShimThroughToOriginal()
        {
            //--------------- Arrange -------------------
            var expected = RandomValueGen.GetRandomInt();
            var original = RandomValueGen.GetAnother(expected);
            var data = new Dictionary<string, object>()
            {
                {"HaveId", new Dictionary<string, object>() {{"Id", original}}}
            };
            var sut = Create(data, typeof(INested));
            var sub = sut.GetPropertyValue("HaveId") as IHaveId;

            //--------------- Assume ----------------
            Expect(sub).Not.To.Be.Null();

            //--------------- Act ----------------------
            sub.Id = expected;

            //--------------- Assert -----------------------
            var cast = data["HaveId"] as Dictionary<string, object>;
            Expect(cast).Not.To.Be.Null();
            Expect(cast)
                .To.Contain.Key("Id")
                .With.Value(expected);
        }


        public interface IEmpty
        {
        }

        private IShimSham Create(
            Dictionary<string, object> data,
            Type interfaceToMimic = null)
        {
            return new DictionaryShimSham(
                // ReSharper disable once CoVariantArrayConversion
                new[] {data},
                interfaceToMimic ?? typeof(IEmpty)
            );
        }
    }
}