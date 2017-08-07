using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

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
    public partial class TestDuckTypingExtensions
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
            Expect(result).To.Be.False();
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
            Expect(result).To.Be.True();
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
            Expect(result1).To.Be.False();
            Expect(result2).To.Be.True();
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
            Expect(error).To.Contain("Mismatched target accessors for Name").And("get -> get/set");
//            Expect(error, Does.Contain("Mismatched target accessors for Name"));
//            Expect(error, Does.Contain("get -> get/set"));
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
            Expect(result1).To.Be.True();
            Expect(result2).To.Be.True();
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
            Expect(result).To.Be.False();
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
            Expect(result).To.Be.True();
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
            Expect(result).To.Be.False();
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
            Expect(result).To.Be.Null();
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
            Expect(result).Not.To.Be.Null();
            Expect(result.Name).To.Equal(expected);
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
            Expect(result).To.Be.Null();
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
            Expect(result).To.Be.True();
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
            Expect(result).Not.To.Be.Null();
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
            Expect(result).Not.To.Be.Null();
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
            Assert.That(() => result.ActorId, Throws.Nothing);
            Assert.That(() => result.TaskId, Throws.Nothing);
            Assert.That(() => result.Payload, Throws.Nothing);
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
            Assert.That(props.Select(p => p.Name), Does.Contain("Id"));
            Assert.That(props.Select(p => p.Name), Does.Contain("Name"));
        }

        public interface IInterfaceWithInterfacedPayload
        {
            IInterfaceWithPayload OuterPayload { get; set; }
        }

        public interface IInterfaceWithPayload
        {
            object Payload { get; }
        }


        [Test]
        public void DuckAs_WhenShouldNotBeAbleToDuckDueToAccessDifferences_ShouldNotDuckSubProp()
        {
            //--------------- Arrange -------------------
            var input = new
            {
                OuterPayload = new
                {
                    Color = "Red"
                } // Interface property is read/write, but this is a property on an anonymous object
            };

            Expect(input.CanDuckAs<IInterfaceWithInterfacedPayload>()).To.Be.False();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.DuckAs<IInterfaceWithInterfacedPayload>();

            //--------------- Assert -----------------------
            Expect(result).To.Be.Null();
        }

        public class RestrictionTest
        {
            private string _writeOnly;
            public string ReadOnly { get; }

            public string WriteOnly
            {
                set => _writeOnly = value;
            }

            public string ReadWrite { get; set; }
        }

        [Test]
        public void IsNoMoreRestrictiveThan()
        {
            // Arrange
            var type = typeof(RestrictionTest);
            var readonlyProp = type.GetProperty("ReadOnly");
            var writeOnlyProp = type.GetProperty("WriteOnly");
            var readWriteProp = type.GetProperty("ReadWrite");

            // Pre-Assert

            // Act
            Expect(readWriteProp.IsNoMoreRestrictiveThan(readonlyProp)).To.Be.True();
            Expect(readWriteProp.IsNoMoreRestrictiveThan(writeOnlyProp)).To.Be.True();

            Expect(readonlyProp.IsNoMoreRestrictiveThan(writeOnlyProp)).To.Be.False();
            Expect(readonlyProp.IsNoMoreRestrictiveThan(readWriteProp)).To.Be.False();

            Expect(writeOnlyProp.IsNoMoreRestrictiveThan(readonlyProp)).To.Be.False();
            Expect(writeOnlyProp.IsNoMoreRestrictiveThan(readWriteProp)).To.Be.False();

            // Assert
        }

        [Test]
        public void IsMoreRestrictiveThan()
        {
            // Arrange
            var type = typeof(RestrictionTest);
            var readonlyProp = type.GetProperty("ReadOnly");
            var writeOnlyProp = type.GetProperty("WriteOnly");
            var readWriteProp = type.GetProperty("ReadWrite");

            // Pre-Assert

            // Act
            Expect(readWriteProp.IsMoreRestrictiveThan(readonlyProp)).To.Be.False();
            Expect(readWriteProp.IsMoreRestrictiveThan(writeOnlyProp)).To.Be.False();

            Expect(readonlyProp.IsMoreRestrictiveThan(writeOnlyProp)).To.Be.True();
            Expect(readonlyProp.IsMoreRestrictiveThan(readWriteProp)).To.Be.True();

            Expect(writeOnlyProp.IsMoreRestrictiveThan(readonlyProp)).To.Be.True();
            Expect(writeOnlyProp.IsMoreRestrictiveThan(readWriteProp)).To.Be.True();

            // Assert
        }

        public class InterfaceWithPayloadImpl : IInterfaceWithPayload
        {
            public object Payload { get; set; }
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
            Expect(inputWithGuid.CanDuckAs<IObjectIdentifier>()).To.Be.True();
            Expect(inputWithGuid.CanDuckAs<IGuidIdentifier>()).To.Be.True();
            Expect(inputWithObject.CanDuckAs<IGuidIdentifier>()).To.Be.False();
            Expect(inputWithObject.CanDuckAs<IObjectIdentifier>()).To.Be.True();

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
            Expect(inputWithGuid.CanFuzzyDuckAs<IObjectIdentifier>()).To.Be.True();
            Expect(inputWithGuid.CanFuzzyDuckAs<IGuidIdentifier>()).To.Be.True();
            Expect(inputWithObject.CanFuzzyDuckAs<IGuidIdentifier>()).To.Be.False();
            Expect(inputWithObject.CanFuzzyDuckAs<IObjectIdentifier>()).To.Be.True();

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
            Expect(ducked.Id).To.Equal(expected);
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
            Expect(ducked).Not.To.Be.Null();
            Expect(ducked.Id).To.Equal(expected);
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
            Expect(ducked).Not.To.Be.Null();
            ducked.Id = newValue;
            Expect(input.id).To.Equal(expected);
            Expect(ducked.Id).To.Equal(newValue);
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
            Expect(ducked).Not.To.Be.Null();
            ducked.Id = newValue;
            Expect(ducked.Id).To.Equal(newValue);
            Expect(input.id).To.Equal(newGuid);
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
            Expect(result).To.Be.False();
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
            Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true))
                .To.Throw<UnDuckableException>();

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
            Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true))
                .To.Throw<UnDuckableException>();

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
            Expect(result).To.Be.True();
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
            Expect(result).To.Be.True();
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
            Expect(result).To.Be.False();
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
            Expect(result).To.Be.True();
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(expectedId);
            Expect(result.Inner).Not.To.Be.Null();
            Expect(result.Inner.Name).To.Equal(expectedName);
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
            Expect(result).To.Be.True();
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(expectedId);
            Expect(result.Inner).Not.To.Be.Null();
            Expect(result.Inner.Name).To.Equal(expectedName);
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
            Expect(result).To.Be.True();
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(expectedId);
            Expect(result.Inner).Not.To.Be.Null();
            Expect(result.Inner.Name).To.Equal(expectedName);
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
            Expect(result).Not.To.Be.Null();
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
            Expect(result).Not.To.Be.Null();
            Expect(result.TaskId).To.Equal(id);
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
            Expect(result).Not.To.Be.Null();
            Expect(result.Statuses).To.Contain.Exactly(1).Equal.To("foo");
            Expect(result.Statuses).To.Contain.Exactly(1).Equal.To("bar");
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
            Expect(propInfo).Not.To.Be.Null();
            var attrib = propInfo.GetCustomAttributes(false).OfType<MooAttribute>().FirstOrDefault();
            Expect(attrib).Not.To.Be.Null();
            Expect(attrib.Dialect).To.Equal("northern");
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
            Expect(propInfo).Not.To.Be.Null();
            var attrib = propInfo.GetCustomAttributes(false).OfType<NamedArgumentAttribute>().FirstOrDefault();

            Expect(attrib).Not.To.Be.Null();
            Expect(attrib.NamedProperty).To.Equal("whizzle");
            Expect(attrib.NamedField).To.Equal("nom");
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

            Expect(attrib).Not.To.Be.Null();
            Expect(attrib.Intent).To.Equal("playful");
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
            Expect(propInfo).Not.To.Be.Null();
            var attrib = propInfo.GetCustomAttributes(true).OfType<DialectAttribute>().FirstOrDefault();
            Expect(attrib).Not.To.Be.Null();
            Expect(attrib.Dialect).To.Equal("Country");
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
            Expect(result).Not.To.Be.Null();
            Expect(result.GetPropertyValue("ActorId")).To.Equal(src.actorId);
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
            Expect(result).Not.To.Be.Null();
            Expect(result.GetPropertyValue("ActorId")).To.Equal(src.ActorId);
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
            Expect(result).Not.To.Be.Null();
            Expect(result.GetPropertyValue("ActorId")).To.Equal(src.ActorId);
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
            Expect(() => src.DuckAs(toType, true))
                .To.Throw<UnDuckableException>();

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

            Expect(result).Not.To.Be.Null();
            Expect(result.GetPropertyValue("ActorId")).To.Equal(src.actoRId);
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

            Expect(() => src.FuzzyDuckAs(toType, true))
                .To.Throw<UnDuckableException>();

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
            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Be.Null();
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Be.Null();
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(input.Id);
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(input.id);
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

            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(expected);
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
            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Equal(expected);
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
            Expect(result).Not.To.Be.Null();
        }

        [Test]
        public void ForceFuzzyDuckAs_GivenEmptyDictionaryAndInterfaceToMimick_WhenCanWriteBack_ShouldWriteBack()
        {
            //--------------- Arrange -------------------
            // TODO: provide a shimming layer so that the input dictionary doesn't have to be case-insensitive to allow write-back
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
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
            Expect(expectedDuck).Not.To.Be.Null();

            //--------------- Act ----------------------
            var result = dict.ForceFuzzyDuckAs<ITravelRequestDetails>();

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Null();
            // ReSharper disable once IsExpressionAlwaysTrue
            Expect(result is ITravelRequestDetails).To.Be.True();
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
                })
                .Not.To.Throw();

            foreach (var prop in result.GetType().GetProperties())
            {
                Expect(dict[prop.Name]).To.Equal(prop.GetValue(result));
            }
        }

        public interface ISillyConfig
        {
            bool Moo { get; }
            bool Cake { get; }
        }

        [Test]
        public void ForceFuzzyDuckAs_Wild()
        {
            // Arrange
            var dict = new Dictionary<string, object>()
            {
                ["moo"] = true
            };
            // Pre-Assert
            // Act
            var result = dict.ForceFuzzyDuckAs<ISillyConfig>();
            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.Moo).To.Be.True();
            Expect(result.Cake).To.Be.False();
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
            Expect(expectedDuck).Not.To.Be.Null();

            //--------------- Act ----------------------
            var result = dict.ForceDuckAs<ITravelRequestDetails>();

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Null();
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
                })
                .Not.To.Throw();

            foreach (var prop in result.GetType().GetProperties())
            {
                Expect(dict[prop.Name]).To.Equal(prop.GetValue(result));
            }
        }

        [Test]
        public void DuckFail_WhenHaveBadDictionaryImplementation_GivingNullKeys_ShouldThrowRecognisableError()
        {
            // Arrange
            var src = new DictionaryWithNullKeys<string, object>();
            // Pre-Assert
            // Act
            Expect(() => src.ForceFuzzyDuckAs<ISillyConfig>())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("Provided dictionary gives null for keys");
            // Assert
        }

        public class DictionaryWithNullKeys<TKey, TValue> : IDictionary<TKey, TValue>
        {
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public int Count { get; }
            public bool IsReadOnly { get; }

            public bool ContainsKey(TKey key)
            {
                throw new NotImplementedException();
            }

            public void Add(TKey key, TValue value)
            {
                throw new NotImplementedException();
            }

            public bool Remove(TKey key)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                throw new NotImplementedException();
            }

            public TValue this[TKey key]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public ICollection<TKey> Keys { get; }
            public ICollection<TValue> Values { get; }
        }


        [Test]
        public void FuzzyDuckOnDefaultDictionary_ShouldWork()
        {
            // Arrange
            var expected = "moo";
            var dict = new DefaultDictionary<string, object>(() => expected);
            // Pre-Assert
            Expect(dict.CanFuzzyDuckAs<IConfig>()).To.Be.True();
            // Act
            var result = dict.FuzzyDuckAs<IConfig>();
            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.BaseUrl).To.Equal(expected);
        }

        public interface IHaveFlags
        {
            bool Flag1 { get; }
            bool Flag2 { get; }
        }

        [Test]
        public void FuzzyDuckOnDefaultDictionary_Part2()
        {
            // Arrange
            var expected = true;
            var dict = new DefaultDictionary<string, object>(() => expected);
            // Pre-Assert
            Expect(dict.CanFuzzyDuckAs<IHaveFlags>()).To.Be.True();
            // Act
            var result = dict.FuzzyDuckAs<IHaveFlags>();
            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.Flag1).To.Be.True();
            Expect(result.Flag2).To.Be.True();
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
            Expect(result.BaseUrl).To.Equal(src["BaseUrl"]);
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
            Expect(result.BaseUrl).To.Equal(src["base url"]);
        }

        [Test]
        public void FuzzyDuckAs_OperatingOnMergeDictionaryWithFallback_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomString();
            var src = new MergeDictionary<string, object>(
                new DefaultDictionary<string, object>(() => expected)
            );

            // Pre-Assert
            Expect(src.CanFuzzyDuckAs<IConfig>()).To.Be.True();

            // Act
            var result = src.FuzzyDuckAs<IConfig>();

            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.BaseUrl).To.Equal(expected);
        }

        [Test]
        public void DuckAs_OperatingOnNameValueCollection_WhenCanDuck_ShouldDuckBothWays()
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
            Expect(result).Not.To.Be.Null();
            Expect(result.BaseUrl).To.Equal(expected1);
            result.BaseUrl = expected2;
            Expect(result.BaseUrl).To.Equal(expected2);
            Expect(src["BaseUrl"]).To.Equal(expected2);
        }

        [TestCase(" ")]
        [TestCase("  ")]
        [TestCase(".")]
        public void FuzzyDuckAs_OperatingOnNameValueCollection_ShouldDuck_WhenSourcePropertyIncludes(string chars)
        {
            // Arrange
            var src = new NameValueCollection();
            var expected = GetRandomHttpUrl();
            src.Add($"base{chars}Url", expected);

            // Pre-Assert

            // Act
            var result = src.FuzzyDuckAs<IConfig>();

            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.BaseUrl).To.Equal(expected);
        }

        [Test]
        public void ForceDuckAs_OperatingOnNameValueCollection_ShouldGoBothWays()
        {
            // Arrange
            var src = new NameValueCollection();
            var expected = GetRandomHttpUrl();

            // Pre-Assert

            // Act
            var result = src.ForceDuckAs<IConfig>();

            // Assert
            Expect(result.BaseUrl).To.Be.Null();
            result.BaseUrl = expected;

            Expect(result.BaseUrl).To.Equal(expected);
            Expect(src["BaseUrl"]).To.Equal(expected);
        }

        public interface IConfigWithUnconfiguredFeature : IConfig
        {
            bool UnconfiguredFeature { get; set; }
        }

        public enum FeatureSets
        {
            Level1,
            Level2
        }

        public class FeatureConfig
        {
            public Guid Id { get; set; }

            public FeatureSets FeatureSet { get; set; }
            public Dictionary<string, bool> Settings { get; set; }
        }


        public interface IExtendedConfig : IConfig
        {
            string Name { get; set; }
        }

        [Test]
        public void ForceFuzzyDuckAs_OperatingOnNameValueCollection_ShouldGoBothWays()
        {
            // Arrange
            var src = new NameValueCollection();
            src["name"] = GetRandomString();
            var expected = GetRandomHttpUrl();

            // Pre-Assert

            // Act
            var result = src.ForceFuzzyDuckAs<IExtendedConfig>();

            // Assert

            Expect(result.Name).To.Equal(src["Name"]);
            Expect(result.BaseUrl).To.Be.Null();
            result.BaseUrl = expected;
            Expect(result.BaseUrl).To.Equal(expected);
            Expect(src["BaseUrl"]).To.Equal(expected);
        }

        [TestFixture]
        public class GivenKeyTransformFunctions : AssertionHelper
        {
            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""));

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctions_AndCantDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(s => "Config." + s,
                    s => s.RegexReplace("Config.", GetRandomString(7)));

                // Assert

                Expect(result, Is.Null);
            }

            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctionsAndMustThrow_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""), true);

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void DuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new NameValueCollection {["Config.BaseUrl"] = expected};

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""));

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void
                DuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndNoThrow_AndCantDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new NameValueCollection {["Config.BaseUrl"] = expected};

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", "moo"));

                // Assert

                Expect(result, Is.Null);
            }

            [Test]
            public void FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.baseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""));

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctions_AndCantDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BasEUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s,
                    s => s.RegexReplace("Config.", GetRandomString(7)));

                // Assert

                Expect(result, Is.Null);
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctionsAndMustThrowIsTrue_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BAseURl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""), true);

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctionsAndMustThrowIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new Dictionary<string, object>
                {
                    ["Config.BAseURlMoo"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""), true),
                    Throws.Exception.InstanceOf<UnDuckableException>());
                // Assert
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new NameValueCollection {["Config.BAseUrl"] = expected};

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""));

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndNoThrow_AndCantDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new NameValueCollection {["Config.BasEUrl"] = expected};

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", "moo"));

                // Assert

                Expect(result, Is.Null);
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndMustThrow_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var data = new NameValueCollection {["Config.BaSeUrl"] = expected};

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""), true);

                // Assert

                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }
        }

        public class GivenKeyPrefix_OperatingOnDictionary : AssertionHelper
        {
            [Test]
            public void FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Null);
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefixAntThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.FuzzyDuckAs<IConfig>(prefix, true),
                    Throws.Exception.InstanceOf<UnDuckableException>());

                // Assert
            }

            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Null);
            }

            [Test]
            public void
                DuckAs_OperatingOnDictionary_WhenGivenKeyPrefixAntThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.DuckAs<IConfig>(prefix, true),
                    Throws.Exception.InstanceOf<UnDuckableException>());

                // Assert
            }
        }

        public class GivenKeyPrefix_OperatingOnNameValueCollection : AssertionHelper
        {
            [Test]
            public void FuzzyDuckAs_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void FuzzyDuckAs_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Null);
            }

            [Test]
            public void
                FuzzyDuckAs_WhenGivenKeyPrefixAntThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.FuzzyDuckAs<IConfig>(prefix, true),
                    Throws.Exception.InstanceOf<UnDuckableException>());

                // Assert
            }

            [Test]
            public void DuckAs_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Not.Null);
                Expect(result.BaseUrl, Is.EqualTo(expected));
            }

            [Test]
            public void DuckAs_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result, Is.Null);
            }

            [Test]
            public void
                DuckAs_WhenGivenKeyPrefixAntThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new NameValueCollection
                {
                    [$"{prefix}BaseUrl1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.DuckAs<IConfig>(prefix, true),
                    Throws.Exception.InstanceOf<UnDuckableException>());

                // Assert
            }
        }

        public class WildFailures
        {
            public interface IBackedByDictionary
            {
                string Name { get; set; }
            }

            public interface IDuckFromDictionaryProperty
            {
                IBackedByDictionary Prop { get; set; }
            }

            public class DuckMe : IDuckFromDictionaryProperty
            {
                public IBackedByDictionary Prop { get; set; }
            }

            public class DuckFromMe
            {
                public Dictionary<string, string> Prop { get; set; }

                public DuckFromMe()
                {
                    Prop = new Dictionary<string, string>();
                }
            }

            [Test]
            public void ShouldBeAbleToAutoDuckDictionaryProperties()
            {
                // Arrange
                var expected = GetRandomString();
                var src = new DuckFromMe();
                src.Prop["Name"] = expected;
                // Pre-Assert
                // Act
                var result = src.FuzzyDuckAs<IDuckFromDictionaryProperty>();
                // Assert

                Expect(result).Not.To.Be.Null();
                Expect(result.Prop.Name).To.Equal(expected);
            }
        }

        [TestFixture]
        public class EnumDucking
        {
            public enum Priorities
            {
                Low,
                Medium,
                High
            }

            public interface IHaveAnEnumProperty
            {
                Priorities Priority { get; }
            }

            [TestFixture]
            public class OperatingOnAnObject
            {
                [Test]
                public void CanDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
                {
                    // Arrange
                    var input = new
                    {
                        Priority = "Medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.CanDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void DuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
                {
                    // Arrange
                    var input = new
                    {
                        Priority = "Medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.DuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.Priority).To.Equal(Priorities.Medium);
                }

                [Test]
                public void CanFuzzyDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
                {
                    // Arrange
                    var input = new
                    {
                        Priority = "MeDium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.CanFuzzyDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void FuzzyDuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
                {
                    // Arrange
                    var input = new
                    {
                        PrIority = "medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FuzzyDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.Priority).To.Equal(Priorities.Medium);
                }
            }

            [TestFixture]
            public class OperatingOnADictionary
            {
                [Test]
                public void CanDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
                {
                    // Arrange
                    var input = new Dictionary<string, object>()
                    {
                        ["Priority"] = "Medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.CanDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void DuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
                {
                    // Arrange
                    var input = new Dictionary<string, object>
                    {
                        ["Priority"] = "Medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.DuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.Priority).To.Equal(Priorities.Medium);
                }

                [Test]
                public void CanFuzzyDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
                {
                    // Arrange
                    var input = new
                    {
                        Priority = "MeDium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.CanFuzzyDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void FuzzyDuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
                {
                    // Arrange
                    var input = new
                    {
                        Priority = "medium"
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FuzzyDuckAs<IHaveAnEnumProperty>();
                    // Assert
                    Expect(result).Not.To.Be.Null();
                    Expect(result.Priority).To.Equal(Priorities.Medium);
                }
            }
        }

        public interface IConnectionStrings
        {
            string SomeString { get; }
        }

        [TestFixture]
        public class OperatingOnConnectionStringSettingsCollection
        {
            [Test]
            public void WhenCanFuzzyDuck_ShouldFuzzyDuck()
            {
                // Arrange
                var expected = GetRandomString(2);
                var setting = new ConnectionStringSettings("some string", expected);
                var settings = new ConnectionStringSettingsCollection() { setting };
                // Pre-Assert
                // Act
                var result = settings.FuzzyDuckAs<IConnectionStrings>();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.SomeString).To.Equal(expected);
            }

            [Test]
            public void WhenCannotFuzzyDuck_GivenShouldThrow_ShouldThrow()
            {
                // Arrange
                var setting = new ConnectionStringSettings("123some string", "some value");
                var settings = new ConnectionStringSettingsCollection() { setting };
                // Pre-Assert
                // Act
                Expect(() => settings.FuzzyDuckAs<IConnectionStrings>(true))
                    .To.Throw<UnDuckableException>();
                // Assert
            }

            [Test]
            public void WhenCanDuck_ShouldFuzzyDuck()
            {
                // Arrange
                var expected = GetRandomString(2);
                var setting = new ConnectionStringSettings("SomeString", expected);
                var settings = new ConnectionStringSettingsCollection() { setting };
                // Pre-Assert
                // Act
                var result = settings.DuckAs<IConnectionStrings>();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.SomeString).To.Equal(expected);
            }

            [Test]
            public void WhenCannotDuck_GivenShouldThrow_ShouldThrow()
            {
                // Arrange
                var setting = new ConnectionStringSettings("MooSomeString", "some value");
                var settings = new ConnectionStringSettingsCollection() { setting };
                // Pre-Assert
                // Act
                Expect(() => settings.DuckAs<IConnectionStrings>(true))
                    .To.Throw<UnDuckableException>();
                // Assert
            }
        }

        public class Merging_CanDuckAsOperations
        {
            public interface IMerge1
            {
                int Id { get; }
                string Name { get; }
            }

            [Test]
            [Ignore("WIP")]
            public void InternalDuckAs_GivenTwoObjectsWhichCouldMergeExactly_ShouldReturnTrue()
            {
                // Arrange
                var obj1 = new {Id = 42};
                var obj2 = new {Name = "Douglas Adams"};
                // Pre-Assert

                // Act
                var result = DuckTypingExtensionSharedMethods.InternalCanDuckAs<IMerge1>(
                    new object[] {obj1, obj2},
                    false,
                    false);

                // Assert
                Expect(result).To.Be.True();
            }
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