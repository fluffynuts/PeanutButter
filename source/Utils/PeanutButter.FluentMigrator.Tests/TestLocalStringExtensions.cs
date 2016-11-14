using System;
using System.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.FluentMigrator.MigrationDumping;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestLocalStringExtensions: AssertionHelper
    {
        [Test]
        public void ContainsOneOf_GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsOneOf(),
                Throws.Exception.InstanceOf<ArgumentException>()
            );
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsOneOf_GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsOneOf(null, "foo"),
                Throws.Exception.InstanceOf<ArgumentException>()
            );
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsOneOf_OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsOneOf("foo");
            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void ContainsOneOf_OperatingOnStringContainingNoneOfTheNeedles_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var input = "foo";
            var search = new[] { "bar", "quuz", "wibbles" };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsOneOf(search);

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void ContainsOneOf_OperatingOnStringContainingOnneOfTheNeedles_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var input = "foo";
            var search = new[] { "bar", "quuz", "oo", "wibbles" }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsOneOf(search);

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }


        [Test]
        public void ContainsAllOf_GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsAllOf(),
                Throws.Exception.InstanceOf<ArgumentException>()
            );
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsAllOf_GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsAllOf(null, "foo"),
                Throws.Exception.InstanceOf<ArgumentException>()
            );
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsAllOf_OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsAllOf("foo");
            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void ContainsAllOf_WhenHaystackContainsAllConstituents_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var input = "hello, world";
            var search = new[] { "hello", ", ", "world" }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsAllOf(search);

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void ContainsAllOf_WhenHaystackMissingNeedle_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var input = "hello, world";
            var search = new[] { "hello", ", ", "there" }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsAllOf(search);

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }
    }
}
