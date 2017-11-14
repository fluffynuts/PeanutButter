using System.Collections.Generic;
using NUnit.Framework;
using PeanutButter.Utils.Dictionaries;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;

namespace PeanutButter.Utils.Tests.Dictionaries
{
    [TestFixture]
    public class TestDictionaryWrappingObject
    {
        [Test]
        public void ShouldImplementIDictionary_string_object()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            Expect(sut).To.Be.An.Instance.Of<IDictionary<string, object>>();
            // Assert
        }

        [Test]
        public void ShouldBeAbleToGetExistingPropertyValue()
        {
            // Arrange
            var src = new { Prop = GetRandomString() };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut["Prop"];
            // Assert
            Expect(result).To.Equal(src.Prop);
        }

        [Test]
        public void ShouldThrowWhenIndexingNonExistentProperty()
        {
            // Arrange
            var src = new { Moo = "Cow" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            Expect(() => sut["Cow"])
                .To.Throw<KeyNotFoundException>()
                .With.Message.Containing("Cow");
            // Assert
        }

        public class HasAField
        {
            public string Name;
        }

        [Test]
        public void ShouldBeAbleToGetExistingFieldValue()
        {
            // Arrange
            var src = new HasAField() { Name = GetRandomString() };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut["Name"];
            // Assert
            Expect(result).To.Equal(src.Name);
        }

        [Test]
        public void ShouldThrowWhenIndexingNonExistentField()
        {
            // Arrange
            var src = GetRandom<HasAField>();
            var sut = Create(src);
            // Pre-Assert
            // Act
            Expect(() => sut["Cow"])
                .To.Throw<KeyNotFoundException>()
                .With.Message.Containing("Cow");
            // Assert
        }

        public class HasAProperty
        {
            public string Prop { get; set; }
        }

        [Test]
        public void ShouldBeAbleToSetAnExistingWritablePropertyWithTheCorrectType()
        {
            // Arrange
            var src = GetRandom<HasAProperty>();
            var sut = Create(src);
            var expected = GetAnother(src.Prop);
            // Pre-Assert
            // Act
            sut["Prop"] = expected;
            // Assert
            Expect(sut["Prop"]).To.Equal(expected);
            Expect(src.Prop).To.Equal(expected);
        }

        [Test]
        public void Keys_ShouldReturnAllKeys()
        {
            // Arrange
            var src = new { One = 1, Two = "two" };
            var expected = new[] { "One", "Two" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Keys;
            // Assert
            Expect(result).To.Be.Deep.Equivalent.To(expected);
        }

        [Test]
        public void Values_ShouldReturnAllValues()
        {
            // Arrange
            var src = new { One = 1, Two = "two" };
            var expected = new object[] { 1, "two" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Values;
            // Assert
            Expect(result).To.Be.Deep.Equivalent.To(expected);
        }

        public class HasAPropertyAndField
        {
            public string Field;
            public bool Property { get; set; }
        }

        [Test]
        public void Count_ShouldReturnPropertyAndFieldCount()
        {
            // Arrange
            var src = GetRandom<HasAPropertyAndField>();
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Count;
            // Assert
            Expect(result).To.Equal(2);
        }

        [Test]
        public void IsReadOnly_ShouldReturnFalse()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            var result = sut.IsReadOnly;
            // Assert
            Expect(result).To.Be.False();
        }

        // TODO: continue from here

        private DictionaryWrappingObject Create(object wrapped = null)
        {
            return new DictionaryWrappingObject(wrapped);
        }
    }
}
