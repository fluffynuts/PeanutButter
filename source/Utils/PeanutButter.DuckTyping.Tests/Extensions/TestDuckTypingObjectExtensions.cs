using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDuckTypingObjectExtensions: AssertionHelper
    {
        public interface IHasReadOnlyName
        {
            string Name { get; }
        }

        [Test]
        public void CanDuckAs_GivenTypeWithOnePropertyAndObjectWhichDoesNotImplement_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var obj = new {
                Id = GetRandomInt()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = obj.CanDuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void CanDuckAs_GivenTypeWithOnePropertyAndObjectImplementingProperty_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var obj = new {
                Name = GetRandomString()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = obj.CanDuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        public interface IHasReadWriteName
        {
            string Name { get; set; }
        }

        public class HasReadOnlyName
        {
            public string Name { get; }
        }

        [Test]
        public void CanDuckAs_ShouldRequireSameReadWritePermissionsOnProperties()
        {
            //--------------- Arrange -------------------
            var obj = new HasReadOnlyName();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = obj.CanDuckAs<IHasReadWriteName>();
            var result2 = obj.CanDuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Expect(result1, Is.False);
            Expect(result2, Is.True);
        }

        public class HasReadWriteNameAndId
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void CanDuckAs_ShouldReturnTrueWhenObjectImplementsMoreThanRequiredInterface()
        {
            //--------------- Arrange -------------------
            var obj = new HasReadWriteNameAndId();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = obj.CanDuckAs<IHasReadOnlyName>();
            var result2 = obj.CanDuckAs<IHasReadWriteName>();

            //--------------- Assert -----------------------
            Expect(result1, Is.True);
            Expect(result2, Is.True);
        }

        interface ICow
        {
            void Moo();
        }

        public class Duck
        {
            public void Quack()
            {
            }
        }

        [Test]
        public void CanDuckAs_ShouldReturnFalseWhenSrcObjectIsMissingInterfaceMethod()
        {
            //--------------- Arrange -------------------
            var src = new Duck();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.CanDuckAs<ICow>();

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        public class Cow
        {
            public void Moo()
            {
            }
        }

        [Test]
        public void CanDuckAs_ShouldReturnTrueWhenRequiredMethodsExist()
        {
            //--------------- Arrange -------------------
            var src = new Cow();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.CanDuckAs<ICow>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        public class AutoCow
        {
            public void Moo(int howManyTimes)
            {
            }
        }

        [Test]
        public void CanDuckAs_ShouldReturnFalseWhenMethodParametersMisMatch()
        {
            //--------------- Arrange -------------------
            var src = new AutoCow();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.CanDuckAs<ICow>();

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }


        [Test]
        public void DuckAs_OperatingOnNull_ShouldReturnNull()
        {
            //--------------- Arrange -------------------
            var src = null as object;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs<ICow>();

            //--------------- Assert -----------------------
            Expect(result, Is.Null);
        }

        [Test]
        public void DuckAs_OperatingDuckable_ShouldReturnDuckTypedWrapper()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString();
            Func<object> makeSource = () =>
                new
                {
                    Name = expected
                };
            var src = makeSource();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Name, Is.EqualTo(expected));
        }

        [Test]
        public void DuckAs_OperatingOnNonDuckable_ShouldReturnNull()
        {
            //--------------- Arrange -------------------
            Func<object> makeSource = () =>
                new
                {
                    Name = GetRandomString()
                };
            var src = makeSource();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs<IHasReadWriteName>();

            //--------------- Assert -----------------------
            Expect(result, Is.Null);
        }





    }
}
