using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDuckTypingObjectExtensions : AssertionHelper
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

        public interface ICow
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
            // ReSharper disable once UnusedParameter.Global
            public void Moo(int howManyTimes)
            {
                /* Empty on purpose */
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
            // ReSharper disable once ExpressionIsAlwaysNull
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
                new {
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
                new {
                    Name = GetRandomString()
                };
            var src = makeSource();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs<IHasReadWriteName>();

            //--------------- Assert -----------------------
            Expect(result, Is.Null);
        }

        [Test]
        public void CanFuzzyDuckAs_OperatingOnSimilarPropertiedThing_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var thing = new { nAmE = GetRandomString() } as object;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = thing.CanFuzzyDuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Assert.IsTrue(result);
        }

        public class LowerCaseCow
        {
            // ReSharper disable once InconsistentNaming
            public void moo()
            {
            }
        }

        [Test]
        public void CanFuzzyDuckAs_OperatingOnSimilarThingWithMethods_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var cow = new LowerCaseCow();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = cow.CanFuzzyDuckAs<ICow>();

            //--------------- Assert -----------------------
            Assert.IsTrue(result);
        }


        [Test]
        public void FuzzyDuckAs_OperatingOnObjectWhichFuzzyMatchesProperties_ShouldReturnFuzzyDuck()
        {
            //--------------- Arrange -------------------
            var src = new {
                nAmE = GetRandomString()
            } as object;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.FuzzyDuckAs<IHasReadOnlyName>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
        }



        [Test]
        public void FuzzyDuckAs_OperatingOnObjectWithFuzzyMatchingMethods_ShouldReturnFuzzyDuck()
        {
            //--------------- Arrange -------------------
            var src = new LowerCaseCow();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.FuzzyDuckAs<ICow>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
        }



        public interface ISomeActivityParameters : IActivityParameters<Guid>
        {
        }

        [Test]
        public void DuckAs_ShouldNotBeConfusedByInterfaceInheritence()
        {
            //--------------- Arrange -------------------
            var src = new {
                ActorId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Payload = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs<ISomeActivityParameters>();

            //--------------- Assert -----------------------
            Expect(() => result.ActorId, Throws.Nothing);
            Expect(() => result.TaskId, Throws.Nothing);
            Expect(() => result.Payload, Throws.Nothing);
        }

        public interface IActivityParameters
        {
            Guid ActorId { get; }
            Guid TaskId { get; }
            void DoNothing();
        }
        public interface IActivityParameters<T> : IActivityParameters
        {
            T Payload { get; }
        }
        public interface ISpecificActivityParameters : IActivityParameters<string>
        {
        }
        public class ActivityParameters : IActivityParameters
        {
            public Guid ActorId { get; }
            public Guid TaskId { get; }
            public void DoNothing()
            {
                /* does nothing */
            }

            public ActivityParameters(Guid actorId, Guid taskId)
            {
                ActorId = actorId;
                TaskId = taskId;
            }
        }

        public class ActivityParameters<T> : ActivityParameters, IActivityParameters<T>
        {
            public T Payload { get; set; }

            public ActivityParameters(Guid actorId, Guid taskId, T payload)
                : base(actorId, taskId)
            {
                Payload = payload;
            }
        }

        [Test]
        public void DuckAs_WildConfusion()
        {
            //--------------- Arrange -------------------
            var instance = new ActivityParameters<string>(Guid.Empty, Guid.Empty, "foo");

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = instance.DuckAs<ISpecificActivityParameters>();

            //--------------- Assert -----------------------
            Assert.IsNotNull(result);
        }



    }
}
