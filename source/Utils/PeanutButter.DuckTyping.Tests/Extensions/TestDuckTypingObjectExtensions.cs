using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable ConvertToLocalFunction
// ReSharper disable InconsistentNaming
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ConstantConditionalAccessQualifier

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
            var obj = new
            {
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
            var obj = new
            {
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

        [Test]
        public void DuckAs_GivenThrowOnErrorIsTrue_WhenHaveReadWriteMismatch_ShouldGiveBackErrorInException()
        {
            //--------------- Arrange -------------------
            var obj = new HasReadOnlyName();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var ex = Assert.Throws<UnDuckableException>(() => obj.DuckAs<IHasReadWriteName>(true));

            //--------------- Assert -----------------------
            var error = ex.Errors.Single();
            Expect(error, Does.Contain("Mismatched target accessors for Name"));
            Expect(error, Does.Contain("get -> get/set"));
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

        [Test]
        public void CanFuzzyDuckAs_OperatingOnSimilarPropertiedThing_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var thing = new {nAmE = GetRandomString()} as object;

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
            var src = new
            {
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
            var src = new
            {
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

        public interface IInterfaceWithPayload
        {
            object Payload { get; set; }
        }

        [Test]
        public void DuckAs_ShouldNotSmashPropertiesOnObjectType()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                Payload = new
                {
                    Id = 1,
                    Name = "Moosicle"
                }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IInterfaceWithPayload>();

            //--------------- Assert -----------------------
            var props = result.Payload.GetType().GetProperties();
            Expect(props.Select(p => p.Name), Does.Contain("Id"));
            Expect(props.Select(p => p.Name), Does.Contain("Name"));
        }

        public interface IInterfaceWithInterfacedPayload
        {
            IInterfaceWithPayload OuterPayload { get; set; }
        }

        [Test]
        public void DuckAs_ShouldNotAllowNonDuckableSubType()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                OuterPayload = new
                {
                    Color = "Red"
                }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IInterfaceWithInterfacedPayload>();

            //--------------- Assert -----------------------
            Assert.IsNotNull(result);
            Assert.IsNull(result.OuterPayload);
        }

        public interface IObjectIdentifier
        {
            object Identifier { get; }
        }

        public interface IGuidIdentifier
        {
            Guid Identifier { get; }
        }

        [Test]
        public void CanDuckAs_ShouldNotTreatGuidAsObject()
        {
            //--------------- Arrange -------------------
            var inputWithGuid = new
            {
                Identifier = new Guid(),
            };
            var inputWithObject = new
            {
                Identifier = new object()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(inputWithGuid.CanDuckAs<IObjectIdentifier>(), Is.True);
            Expect(inputWithGuid.CanDuckAs<IGuidIdentifier>(), Is.True);
            Expect(inputWithObject.CanDuckAs<IGuidIdentifier>(), Is.False);
            Expect(inputWithObject.CanDuckAs<IObjectIdentifier>(), Is.True);

            //--------------- Assert -----------------------
        }

        [Test]
        public void CanFuzzyDuckAs_ShouldNotTreatGuidAsObject()
        {
            //--------------- Arrange -------------------
            var inputWithGuid = new
            {
                identifier = new Guid(),
            };
            var inputWithObject = new
            {
                identifier = new object()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(inputWithGuid.CanFuzzyDuckAs<IObjectIdentifier>(), Is.True);
            Expect(inputWithGuid.CanFuzzyDuckAs<IGuidIdentifier>(), Is.True);
            Expect(inputWithObject.CanFuzzyDuckAs<IGuidIdentifier>(), Is.False);
            Expect(inputWithObject.CanFuzzyDuckAs<IObjectIdentifier>(), Is.True);

            //--------------- Assert -----------------------
        }

        public interface IWithStringId
        {
            string Id { get; set; }
        }

        [Test]
        public void FuzzyDuckAs_WhenReadingProperty_ShouldBeAbleToConvertBetweenGuidAndString()
        {
            //--------------- Arrange -------------------
            var input = new WithGuidId()
            {
                id = Guid.NewGuid()
            };
            var expected = input.id.ToString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var ducked = input.FuzzyDuckAs<IWithStringId>();

            //--------------- Assert -----------------------
            Assert.IsNotNull(ducked);
            Expect(ducked.Id, Is.EqualTo(expected));
        }

        public interface IWithGuidId
        {
            Guid Id { get; set; }
        }

        public class WithGuidId
        {
            public Guid id { get; set; }
        }

        public class WithStringId
        {
            public string id { get; set; }
        }

        [Test]
        public void FuzzyDuckAs_WhenReadingProperty_ShouldBeAbleToConvertFromStringToGuid()
        {
            //--------------- Arrange -------------------
            var expected = Guid.NewGuid();
            var input = new WithStringId()
            {
                id = expected.ToString()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var ducked = input.FuzzyDuckAs<IWithGuidId>();

            //--------------- Assert -----------------------
            Expect(ducked, Is.Not.Null);
            Expect(ducked.Id, Is.EqualTo(expected));
        }

        [Test]
        public void FuzzyDuckAs_WhenWritingProperty_ShouldBeAbleToConvertFromGuidToString()
        {
            //--------------- Arrange -------------------
            var newValue = Guid.NewGuid();
            var expected = newValue.ToString();
            var input = new WithStringId()
            {
                id = GetRandomString()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var ducked = input.FuzzyDuckAs<IWithGuidId>();

            //--------------- Assert -----------------------
            Expect(ducked, Is.Not.Null);
            ducked.Id = newValue;
            Expect(input.id, Is.EqualTo(expected));
            Expect(ducked.Id, Is.EqualTo(newValue));
        }

        [Test]
        public void FuzzyDuckAs_WhenWritingProperty_ShouldBeAbleToConvertFromValidGuidStringToGuid()
        {
            //--------------- Arrange -------------------
            var newGuid = Guid.NewGuid();
            var newValue = newGuid.ToString();
            var input = new WithGuidId();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var ducked = input.FuzzyDuckAs<IWithStringId>();

            //--------------- Assert -----------------------
            Expect(ducked, Is.Not.Null);
            ducked.Id = newValue;
            Expect(ducked.Id, Is.EqualTo(newValue));
            Expect(input.id, Is.EqualTo(newGuid));
        }


        public interface IHasAnActorId
        {
            Guid ActorId { get; }
        }

        public interface IActivityParametersInherited : IHasAnActorId
        {
            Guid TaskId { get; }
        }

        [Test]
        public void CanFuzzyDuckAs_ShouldFailWhenExpectedToFail()
        {
            //--------------- Arrange -------------------
            var parameters = new
            {
                travellerId = new Guid(), // should be actorId!
                taskId = new Guid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = parameters.CanFuzzyDuckAs<IActivityParametersInherited>();

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void FuzzyDuckAs_NonGeneric_ActingOnObject_ShouldThrowWhenInstructedToAndFailingToDuck()
        {
            //--------------- Arrange -------------------
            var parameters = new
            {
                travellerId = new Guid(), // should be actorId!
                taskId = new Guid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true),
                Throws.Exception.InstanceOf<UnDuckableException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void FuzzyDuckAs_NonGeneric_ActingOnDictionary_ShouldThrowWhenInstructedToAndFailingToDuck()
        {
            //--------------- Arrange -------------------
            var parameters = new Dictionary<string, object>()
            {
                {"travellerId", new Guid()},
                {"taskId", new Guid()}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true),
                Throws.Exception.InstanceOf<UnDuckableException>());

            //--------------- Assert -----------------------
        }

        public interface IDictionaryInner
        {
            string Name { get; set; }
        }

        public interface IDictionaryOuter
        {
            int Id { get; set; }
            IDictionaryInner Inner { get; set; }
        }


        [Test]
        public void
            CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenAllPropertiesAreFound_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString();
            var data = new Dictionary<string, object>()
                {{"Name", expected}};

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = data.CanDuckAs<IDictionaryInner>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void
            CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenNullablePropertyIsFound_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>()
                {{"Name", null}};

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = data.CanDuckAs<IDictionaryInner>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        public interface IHaveId
        {
            int Id { get; set; }
        }

        [Test]
        public void
            CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenNonNullablePropertyIsFoundAsNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>()
                {{"Id", null}};

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = data.CanDuckAs<IHaveId>();

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void CanDuckAs_OperatingOnMultiLevelDictionary_WhenAllPropertiesFound_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var data = new Dictionary<string, object>()
            {
                {"Id", GetRandomInt()},
                {"Inner", new Dictionary<string, object>() {{"Name", GetRandomString()}}}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = data.CanDuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void DuckAs_OperatingOnDictionaryOfStringAndObject_WhenIsDuckable_ShouldDuck()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();
            var input = new Dictionary<string, object>()
            {
                {"Id", expectedId},
                {"Inner", new Dictionary<string, object>() {{"Name", expectedName}}}
            };
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(expectedId));
            Expect(result.Inner, Is.Not.Null);
            Expect(result.Inner.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void CanFuzzyDuckAs_OperatingOnAppropriateCaseInsensitiveDictionary_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();
            var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {"id", expectedId},
                {"inner", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {{"nAmE", expectedName}}}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.CanFuzzyDuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnCaseInsensitiveDictionary_ShouldWork()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();
            var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {"id", expectedId},
                {"inner", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {{"nAmE", expectedName}}}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(expectedId));
            Expect(result.Inner, Is.Not.Null);
            Expect(result.Inner.Name, Is.EqualTo(expectedName));
        }


        [Test]
        public void CanFuzzyDuckAs_OperatingOnWouldBeAppropriateCaseSensitiveDictionary_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();
            var input = new Dictionary<string, object>()
            {
                {"id", expectedId},
                {"inner", new Dictionary<string, object>() {{"nAmE", expectedName}}}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.CanFuzzyDuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnCaseDifferentCaseSensitiveDictionary_ShouldReturnObject()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();
            var input = new Dictionary<string, object>()
            {
                {"id", expectedId},
                {"inner", new Dictionary<string, object>() {{"nAmE", expectedName}}}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IDictionaryOuter>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(expectedId));
            Expect(result.Inner, Is.Not.Null);
            Expect(result.Inner.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void DuckAs_IssueSeenInWildShouldNotHappen()
        {
            //--------------- Arrange -------------------
            var instance = new ActivityParameters<string>(Guid.Empty, Guid.Empty, "foo");

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = instance.DuckAs<ISpecificActivityParameters>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
        }

        public interface IHasAGuid
        {
            Guid TaskId { get; set; }
        }

        [Test]
        public void FuzzyDuckAs_ShouldBeAbleToDuckADictionaryWithConvertableTypes()
        {
            //--------------- Arrange -------------------
            var id = Guid.NewGuid();
            var data = new Dictionary<string, object>()
            {
                {"taskId", id.ToString()}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = data.FuzzyDuckAs<IHasAGuid>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.TaskId, Is.EqualTo(id));
        }

        public interface IWorkflowTaskStatusFilters
        {
            string[] Statuses { get; }
        }


        [Test]
        public void FuzzyDuckAs_ShouldBeAbleToDuckSimpleObjectWithStringArray()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                Statuses = new[] {"foo", "bar"}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IWorkflowTaskStatusFilters>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Statuses, Does.Contain("foo"));
            Expect(result.Statuses, Does.Contain("bar"));
        }

        public class MooAttribute : Attribute
        {
            public string Dialect { get; }

            public MooAttribute(string dialect)
            {
                Dialect = dialect;
            }
        }

        public class WoofAttribute : Attribute
        {
            public string Intent { get; }

            public WoofAttribute(string intent)
            {
                Intent = intent;
            }
        }

        public class NamedArgumentAttribute : Attribute
        {
            public string NamedProperty { get; set; }
            public string NamedField;
        }

        [Woof("playful")]
        public interface IHasCustomAttributes
        {
            [Moo("northern")]
            [NamedArgument(NamedProperty = "whizzle", NamedField = "nom")]
            string Name { get; }
        }

        [Test]
        public void DuckAs_ShouldCopyCustomAttributes_OnProperties()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                name = "cow"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasCustomAttributes>();

            //--------------- Assert -----------------------
            var propInfo = result.GetType().GetProperty("Name");
            Expect(propInfo, Is.Not.Null);
            var attrib = propInfo.GetCustomAttributes(false).OfType<MooAttribute>().FirstOrDefault();
            Expect(attrib, Is.Not.Null);
            Expect(attrib?.Dialect, Is.EqualTo("northern"));
        }

        [Test]
        public void DuckAs_ShouldCopyNamedArgumentCustomAttributes_OnProperties()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                name = "cow"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasCustomAttributes>();

            //--------------- Assert -----------------------
            var propInfo = result.GetType().GetProperty("Name");
            Expect(propInfo, Is.Not.Null);
            var attrib = propInfo.GetCustomAttributes(false).OfType<NamedArgumentAttribute>().FirstOrDefault();
            Expect(attrib, Is.Not.Null);
            Expect(attrib?.NamedProperty, Is.EqualTo("whizzle"));
            Expect(attrib?.NamedField, Is.EqualTo("nom"));
        }

        [Test]
        public void DuckAs_ShouldCopyCustomAttributes_OnTheInterface()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                name = "cow"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasCustomAttributes>();

            //--------------- Assert -----------------------
            var attrib = result.GetType().GetCustomAttributes(false).OfType<WoofAttribute>().FirstOrDefault();
            Expect(attrib, Is.Not.Null);
            Expect(attrib?.Intent, Is.EqualTo("playful"));
        }

        public class DialectAttribute : Attribute
        {
            public string Dialect { get; set; }

            public DialectAttribute(string dialect)
            {
                Dialect = dialect;
            }
        }

        public interface IRegionSpecificCow
        {
            [Dialect("Country")]
            string Moo { get; }
        }

        [Test]
        public void DuckAs_ShouldCopyComplexCustomAttributes()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                moo = "Moo, eh"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IRegionSpecificCow>();

            //--------------- Assert -----------------------
            var propInfo = result.GetType().GetProperty("Moo");
            Expect(propInfo, Is.Not.Null);
            var attrib = propInfo.GetCustomAttributes(true).OfType<DialectAttribute>().FirstOrDefault();
            Expect(attrib, Is.Not.Null);
            Expect(attrib.Dialect, Is.EqualTo("Country"));
        }


        [Test]
        public void FuzzyDuckAsNonGeneric_ShouldDuckWhenPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                actorId = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.FuzzyDuckAs(toType);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.GetPropertyValue("ActorId"), Is.EqualTo(src.actorId));
        }

        [Test]
        public void DuckAsNonGeneric_ShouldDUckWhenPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                ActorId = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs(toType);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.GetPropertyValue("ActorId"), Is.EqualTo(src.ActorId));
        }

        [Test]
        public void DuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldDuckWhenPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                ActorId = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.DuckAs(toType, true);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.GetPropertyValue("ActorId"), Is.EqualTo(src.ActorId));
        }

        [Test]
        public void DuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldThrowWhenNotPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                bob = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.DuckAs(toType, true),
                Throws.Exception.InstanceOf<UnDuckableException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void FuzzyDuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldDuckWhenPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                actoRId = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.FuzzyDuckAs(toType, true);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.GetPropertyValue("ActorId"), Is.EqualTo(src.actoRId));
        }

        [Test]
        public void FuzzyDuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldThrowWhenNotPossible()
        {
            //--------------- Arrange -------------------
            var toType = typeof(IHasAnActorId);
            var src = new
            {
                bob = Guid.NewGuid()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.FuzzyDuckAs(toType, true),
                Throws.Exception.InstanceOf<UnDuckableException>());

            //--------------- Assert -----------------------
        }

        public interface IHasNullableId
        {
            Guid? Id { get; set; }
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnDictionary_WhenSourcePropertyIsNullableAndMissing_SHouldDuckAsNullProperty()
        {
            //--------------- Arrange -------------------
            var input = new Dictionary<string, object>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasNullableId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.Null);
        }

        [Test]
        public void DuckAs_OperatingOnDictionary_WhenSourcePropertyIsNullableAndMissing_SHouldDuckAsNullProperty()
        {
            //--------------- Arrange -------------------
            var input = new Dictionary<string, object>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IHasNullableId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.Null);
        }

        public interface IHasNullableReadonlyId
        {
            Guid? Id { get; }
        }

        [Test]
        public void
            DuckAs_OperatingOnObjectWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadOnlyProperty_ShouldDuck()
        {
            //--------------- Arrange -------------------
            var input = new {Id = Guid.NewGuid()};

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IHasNullableReadonlyId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(input.Id));
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnObjectWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadOnlyProperty_ShouldDuck()
        {
            //--------------- Arrange -------------------
            var input = new {id = Guid.NewGuid()};

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasNullableReadonlyId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(input.id));
        }

        [Test]
        public void
            DuckAs_OperatingOnDictionaryWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadonlyProperty_ShouldDuck()
        {
            //--------------- Arrange -------------------
            var expected = Guid.NewGuid();
            var input = new Dictionary<string, object>()
            {
                {"Id", expected}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IHasNullableReadonlyId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(expected));
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnDictionaryWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadonlyProperty_ShouldFuzzyDuck()
        {
            //--------------- Arrange -------------------
            var expected = Guid.NewGuid();
            var input = new Dictionary<string, object>()
            {
                {"Id", expected}
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.FuzzyDuckAs<IHasNullableReadonlyId>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result.Id, Is.EqualTo(expected));
        }


        [Test]
        public void FailingWildDuck1()
        {
            //--------------- Arrange -------------------
            var json =
                "{\"flowId\":\"Travel Request\",\"activityId\":\"Capture Travel Request Details\",\"payload\":{\"taskId\":\"4e53c85b-ca72-4c12-b185-50342ed0fc30\",\"payload\":{\"Initiated\":\"\",\"DepartingFrom\":\"123\",\"TravellingTo\":\"123\",\"Departing\":\"\",\"PreferredDepartureTime\":\"\",\"Returning\":\"\",\"PreferredReturnTime\":\"\",\"ReasonForTravel\":\"123\",\"CarRequired\":\"\",\"AccomodationRequired\":\"\",\"AccommodationNotes\":\"213\"}}}";
            var jobject = JsonConvert.DeserializeObject<JObject>(json);
            var dict = jobject.ToDictionary();
            (dict["payload"] as Dictionary<string, object>)["actorId"] = Guid.Empty.ToString();
            var payload = dict["payload"];

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = payload.FuzzyDuckAs<ITravelRequestCaptureDetailsActivityParameters>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
        }

        [Test]
        public void ForceFuzzyDuckAs_GivenEmptyDictionaryAndInterfaceToMimick_ShouldHandleIt()
        {
            //--------------- Arrange -------------------
            var dict = new Dictionary<string, object>();
            var expected = new TravelRequestDetails()
            {
                Initiated = GetRandomDate(),
                DepartingFrom = GetRandomString(),
                TravellingTo = GetRandomString(),
                Departing = GetRandomDate(),
                PreferredDepartureTime = GetRandomString(),
                Returning = GetRandomDate(),
                PreferredReturnTime = GetRandomString(),
                ReasonForTravel = GetRandomString(),
                CarRequired = GetRandomBoolean(),
                AccomodationRequired = GetRandomBoolean(),
                AccommodationNotes = GetRandomString()
            };
            var expectedDuck = expected.DuckAs<ITravelRequestDetails>();

            //--------------- Assume ----------------
            Expect(expectedDuck, Is.Not.Null);

            //--------------- Act ----------------------
            var result = dict.ForceFuzzyDuckAs<ITravelRequestDetails>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result, Is.InstanceOf<ITravelRequestDetails>());
            Expect(() =>
            {
                result.Initiated = expectedDuck.Initiated;
                result.DepartingFrom = expectedDuck.DepartingFrom;
                result.TravellingTo = expectedDuck.TravellingTo;
                result.Departing = expectedDuck.Departing;
                result.PreferredDepartureTime = expectedDuck.PreferredDepartureTime;
                result.Returning = expectedDuck.Returning;
                result.PreferredReturnTime = expectedDuck.PreferredReturnTime;
                result.ReasonForTravel = expectedDuck.ReasonForTravel;
                result.CarRequired = expectedDuck.CarRequired;
                result.AccomodationRequired = expectedDuck.AccomodationRequired;
                result.AccommodationNotes = expectedDuck.AccommodationNotes;
            }, Throws.Nothing);

            foreach (var prop in result.GetType().GetProperties())
            {
                Expect(dict[prop.Name], Is.EqualTo(prop.GetValue(result)));
            }
        }

        [Test]
        public void ForceDuckAs_GivenEmptyDictionaryAndInterfaceToMimick_ShouldHandleIt()
        {
            //--------------- Arrange -------------------
            var dict = new Dictionary<string, object>();
            var expected = new TravelRequestDetails()
            {
                Initiated = GetRandomDate(),
                DepartingFrom = GetRandomString(),
                TravellingTo = GetRandomString(),
                Departing = GetRandomDate(),
                PreferredDepartureTime = GetRandomString(),
                Returning = GetRandomDate(),
                PreferredReturnTime = GetRandomString(),
                ReasonForTravel = GetRandomString(),
                CarRequired = GetRandomBoolean(),
                AccomodationRequired = GetRandomBoolean(),
                AccommodationNotes = GetRandomString()
            };
            var expectedDuck = expected.DuckAs<ITravelRequestDetails>();

            //--------------- Assume ----------------
            Expect(expectedDuck, Is.Not.Null);

            //--------------- Act ----------------------
            var result = dict.ForceDuckAs<ITravelRequestDetails>();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result, Is.InstanceOf<ITravelRequestDetails>());
            Expect(() =>
            {
                result.Initiated = expectedDuck.Initiated;
                result.DepartingFrom = expectedDuck.DepartingFrom;
                result.TravellingTo = expectedDuck.TravellingTo;
                result.Departing = expectedDuck.Departing;
                result.PreferredDepartureTime = expectedDuck.PreferredDepartureTime;
                result.Returning = expectedDuck.Returning;
                result.PreferredReturnTime = expectedDuck.PreferredReturnTime;
                result.ReasonForTravel = expectedDuck.ReasonForTravel;
                result.CarRequired = expectedDuck.CarRequired;
                result.AccomodationRequired = expectedDuck.AccomodationRequired;
                result.AccommodationNotes = expectedDuck.AccommodationNotes;
            }, Throws.Nothing);

            foreach (var prop in result.GetType().GetProperties())
            {
                Expect(dict[prop.Name], Is.EqualTo(prop.GetValue(result)));
            }
        }

        public interface IConfig
        {
            string BaseUrl { get; set; }
        }

        [Test]
        public void DuckAs_OperatingOnStringStringDictionary_WhenCanDuck_ShouldDuck()
        {
            // Arrange
            var src = new Dictionary<string, string>()
            {
                ["BaseUrl"] = GetRandomHttpUrl()
            };

            // Pre-Assert

            // Act
            var result = src.DuckAs<IConfig>(true);

            // Assert
            Expect(result.BaseUrl, Is.EqualTo(src["BaseUrl"]));
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnStringStringDictionary_ShouldForgiveWhitespaceInPropertyNames()
        {
            // Arrange
            var src = new Dictionary<string, string>()
            {
                ["base url"] = GetRandomHttpUrl()
            };

            // Pre-Assert

            // Act
            var result = src.FuzzyDuckAs<IConfig>();

            // Assert
            Expect(result.BaseUrl, Is.EqualTo(src["base url"]));
        }

        [Test]
        public void DuckAs_OperatingOnNameValueCollection_WhenCanDuck_ShouldDuck()
        {
            // Arrange
            var src = new NameValueCollection();
            var expected1 = GetRandomHttpUrl();
            var expected2 = GetRandom(o => o != expected1, GetRandomHttpUrl);
            src.Add("BaseUrl", expected1);

            // Pre-Assert

            // Act
            var result = src.DuckAs<IConfig>();

            // Assert
            Expect(result, Is.Not.Null);
            Expect(result.BaseUrl, Is.EqualTo(expected1));
            result.BaseUrl = expected2;
            Expect(result.BaseUrl, Is.EqualTo(expected2));
            Expect(src["BaseUrl"], Is.EqualTo(expected2));
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnNameValueCollection_WhenCanDuck_ShouldDuck()
        {
            // Arrange
            var src = new NameValueCollection();
            var expected = GetRandomHttpUrl();
            src.Add("base Url", expected);

            // Pre-Assert

            // Act
            var result = src.FuzzyDuckAs<IConfig>();

            // Assert
            Expect(result, Is.Not.Null);
            Expect(result.BaseUrl, Is.EqualTo(expected));
        }

        public class TravelRequestDetails : ITravelRequestDetails
        {
            public DateTime Initiated { get; set; }
            public string DepartingFrom { get; set; }
            public string TravellingTo { get; set; }
            public DateTime Departing { get; set; }
            public string PreferredDepartureTime { get; set; }
            public DateTime Returning { get; set; }
            public string PreferredReturnTime { get; set; }
            public string ReasonForTravel { get; set; }
            public bool CarRequired { get; set; }
            public bool AccomodationRequired { get; set; }
            public string AccommodationNotes { get; set; }
        }

        public interface ITravelRequestDetails
        {
            DateTime Initiated { get; set; }
            string DepartingFrom { get; set; }
            string TravellingTo { get; set; }
            DateTime Departing { get; set; }
            string PreferredDepartureTime { get; set; }
            DateTime Returning { get; set; }
            string PreferredReturnTime { get; set; }
            string ReasonForTravel { get; set; }
            bool CarRequired { get; set; }
            bool AccomodationRequired { get; set; }
            string AccommodationNotes { get; set; }
        }

        public interface ITravelRequestCaptureDetailsActivityParameters :
            IActivityParameters<ITravelRequestDetails>
        {
        }

        public interface IActivityParameters : IHasAnActorId
        {
            Guid TaskId { get; }
        }

        public interface IActivityParameters<out T> : IActivityParameters
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

        public interface ICreateMe
        {
            int Id { get; set; }
            string Name { get; set; }
        }
    }
}