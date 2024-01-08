using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable UnusedVariable

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestTypeExtensions
    {
        public class HasNoActions;

        public class HasMethodCalledFoo
        {
            public int Foo()
            {
                return -1;
            }
        }

        public class HasActionCalledFoo
        {
            public void Foo()
            {
            }
        }

        [Test]
        public void HasActionWithName_GivenTypeWithoutAction_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasNoActions);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.HasActionMethodWithName(GetRandomAlphaString(2, 10));

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.False();
        }

        [Test]
        public void HasActionWithName_GivenTypeWithMethodReturningValueByThatName_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasMethodCalledFoo);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.HasActionMethodWithName("Foo");

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.False();
        }

        [Test]
        public void HasActionWithName_GivenTypeWithActionByThatName_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasActionCalledFoo);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.HasActionMethodWithName("Foo");

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithoutAction_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasNoActions);
            var actionName = GetRandomAlphaString(2, 10);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldHaveActionMethodWithName(actionName))
                .To.Throw<AssertionException>()
                .With.Message.Equal.To(
                    "Expected to find method '" + actionName + "' on type 'HasNoActions' but didn't."
                );

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithMethodReturningValueByThatName_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasMethodCalledFoo);
            var actionName = "Foo";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldHaveActionMethodWithName(actionName))
                .To.Throw<AssertionException>()
                .With.Message.Equal.To(
                    "Expected to find method '" + actionName + "' on type 'HasMethodCalledFoo' but didn't."
                );

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithActionByThatName_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasActionCalledFoo);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldHaveActionMethodWithName("Foo")).Not.To.Throw();

            //---------------Test Result -----------------------
        }

        public interface IInterface;

        public class DoesNotImplement;

        public class DoesImplement : IInterface;

        [Test]
        public void ShouldImplement_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldImplement<Tests.IInterface>())
                .To.Throw<AssertionException>()
                .With.Message.Equal.To(
                    "DoesNotImplement should implement IInterface"
                );

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldImplement_WhenTypeDoesImplementOtherInterface_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => typeof(DoesImplement).ShouldImplement<IInterface>()
                )
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        public class DoesImplementDerivative : DoesImplement;

        [Test]
        public void ShouldImplement_WhenRequestedTypeIsAClass_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        typeof(DoesImplementDerivative).ShouldImplement<DoesImplement>()
                )
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldInheritFrom_WhenRequestedTypeIsABaseClass_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => typeof(DoesImplementDerivative).ShouldInheritFrom<DoesImplement>())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        public class AncestorClass;

        public class DescendantClass : AncestorClass;

        [Test]
        public void ShouldNotInheritFrom_generic_WhenRequestedTypeIsInTheInheritenceHeirachy_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(DescendantClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotInheritFrom<AncestorClass>())
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotInheritFrom_WhenRequestedTypeIsInTheInheritenceHeirachy_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(DescendantClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotInheritFrom(typeof(AncestorClass)))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }
        // TODO: convert the rest of these assertions to NExpect

        [Test]
        public void ShouldInheritFrom_WhenRequestedTypeIsAnInterface_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(
                () =>
                    typeof(DoesImplementDerivative).ShouldInheritFrom<Tests.IInterface>()
            );

            //---------------Test Result -----------------------
        }


        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldBeAssignableFrom(typeof(IInterface)))
                .To.Throw<AssertionException>()
                .With.Message.Equal.To("DoesNotImplement should implement IInterface");

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => typeof(DoesImplement).ShouldBeAssignableFrom(typeof(IInterface)))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplement_WhenTypeDoesNotImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldNotImplement<IInterface>())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplement_WhenTypeDoesImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => typeof(DoesImplement)
                        .ShouldNotImplement<IInterface>()
                ).To.Throw<AssertionException>()
                .With.Message.Equal.To("DoesImplement should not implement IInterface");

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplementWithArguments_WhenTypeDoesNotImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldNotBeAssignableFrom(typeof(IInterface)))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => typeof(DoesImplement)
                        .ShouldNotBeAssignableFrom(typeof(IInterface))
                )
                .To.Throw<AssertionException>()
                .With.Message.Equal.To("DoesImplement should not implement IInterface");

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldIImplementm_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldImplement<IInterface>())
                .To.Throw<AssertionException>()
                .With.Message.Equal.To("DoesNotImplement should implement IInterface");

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldImplement_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => typeof(DoesImplement).ShouldImplement<IInterface>())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void PrettyName_GivenNonGenericType_ShouldReturnOnlyBaseNameForIt()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = typeof(DoesImplement).PrettyName();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal("DoesImplement");
        }

        // ReSharper disable once UnusedTypeParameter
        public class GenericType<T>;

        [Test]
        public void PrettyName_GIvenGenericType_ShouldReturnReadableGenericName()
        {
            //---------------Set up test pack-------------------
            var t = typeof(GenericType<string>);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.PrettyName();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal("TestTypeExtensions+GenericType<String>");
        }

        public class CanHaveNullParameter
        {
            public CanHaveNullParameter(string arg)
            {
            }
        }

        [Test]
        public void
            ShouldThrowWhenConstructorParameterIsNull_WhenGivenTypeWithParameterSetToNullAndConstructorDoesNotThrow_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CanHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                () => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(string))
            ).To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        public class CannotHaveNullParameter
        {
            public CannotHaveNullParameter(string arg)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException(nameof(arg));
                }
            }
        }

        [Test]
        public void ShouldThrowWhenConstructorParameterIsNull_GivenTypeWhichChecksForNull_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CannotHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(string)))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void
            ShouldThrowWhenConstructorParameterIsNull_GivenTypeWhichChecksForNull_ShouldThrowIfParameterTypeDoesNotMatch()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CannotHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(int)))
                .To.Throw<AssertionException>()
                .With.Message.Equal.To(
                    "Parameter arg is expected to have type: 'Int32' but actually has type: 'String'"
                );

            //---------------Test Result -----------------------
        }

        public abstract class AbstractThing;

        [Test]
        public void ShouldBeAbstract_GivenAbstractType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AbstractThing);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldBeAbstract())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        public class NotAnAbstractThing : AbstractThing;

        [Test]
        public void ShouldBeAbstract_GivenNonAbstractType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(NotAnAbstractThing);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldBeAbstract())
                .To.Throw<AssertionException>()
                .With.Message.Containing(sut.Name)
                .And.Containing("should be abstract");

            //---------------Test Result -----------------------
        }

        public class ClassWithAProperty
        {
            public int IntProperty { get; set; }
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameOnly_WhenPropertyExists_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveProperty("IntProperty"))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenInvalidNameOnly_WhenPropertyExists_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveProperty("IntProperty1"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameAndType_WhenPropertyExistsWithExpectedType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveProperty("IntProperty", typeof(int)))
                .Not.To.Throw();
            Expect(() => sut.ShouldHaveProperty<int>("IntProperty"))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameAndInvalidType_WhenPropertyExistsWithDifferentType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveProperty("IntProperty", typeof(string)))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveProperty<string>("IntProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenInvalidNameOnly_WhenPropertyDoesNotExist_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotHaveProperty("StringProperty"))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenNameOnly_WhenPropertyExists_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotHaveProperty("IntProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenNameAndType_WhenPropertyExistsWithType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotHaveProperty("IntProperty", typeof(int)))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldNotHaveProperty<int>("IntProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenValidNameAndType_WhenPropertyExistsWithDifferentType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldNotHaveProperty("IntProperty", typeof(string)))
                .Not.To.Throw();
            Expect(() => sut.ShouldNotHaveProperty<string>("IntProperty"))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        public class ReadOnlyPropertyTestClass
        {
            public int ReadOnlyProperty { get; }
            public int ReadWriteProperty { get; set; }
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsReadOnly_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty"))
                .Not.To.Throw();
            Expect(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty", typeof(int)))
                .Not.To.Throw();
            Expect(() => sut.ShouldHaveReadOnlyProperty<int>("ReadOnlyProperty"))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsNotReadOnly_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveReadOnlyProperty("ReadWriteProperty"))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveReadOnlyProperty("ReadWriteProperty", typeof(int)))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveReadOnlyProperty<int>("ReadWriteProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyDoesNotExistByName_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveReadOnlyProperty("SomeProperty"))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveReadOnlyProperty("SomeProperty", typeof(int)))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveReadOnlyProperty<int>("SomeProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsReadOnlyButHasDifferentType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty", typeof(string)))
                .To.Throw<AssertionException>();
            Expect(() => sut.ShouldHaveReadOnlyProperty<bool>("ReadOnlyProperty"))
                .To.Throw<AssertionException>();

            //---------------Test Result -----------------------
        }

        public class ClassForTestingMethodAssertions
        {
            public void MethodA()
            {
            }

            internal void MethodB()
            {
            }
        }

        [Test]
        public void ShouldHaveNonPublicMethod_OperatingOnType_GivenMethodName_WhenDoesNotHaveMethodAtAll_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassForTestingMethodAssertions);
            var search = GetRandomString(10, 15);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveNonPublicMethod(search))
                .To.Throw<AssertionException>()
                .With.Message.Containing("Method not found")
                .And.Containing(search);

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveNonPublicMethod_OperatingOnType_GivenMethodName_WhenSearchMethodIsPublic_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassForTestingMethodAssertions);
            var search = "MethodA";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveNonPublicMethod(search))
                .To.Throw<AssertionException>()
                .With.Message.Equal.To(
                    $"Expected method '{search}' not to be public"
                );

            //---------------Test Result -----------------------
        }

        [Test]
        public void
            ShouldHaveNonPublicMethod_OperatingOnType_GivenMethodName_WhenSearchMethodIsNotPublic_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassForTestingMethodAssertions);
            var search = "MethodB";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => sut.ShouldHaveNonPublicMethod(search))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [TestCase(typeof(IEnumerable<string>), true)]
        [TestCase(typeof(ICollection<string>), true)]
#pragma warning disable S100 // Methods and properties should be named in camel case
        public void ImplementsEnumerableGenericType_(Type toTest, bool expected)
#pragma warning restore S100 // Methods and properties should be named in camel case
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = toTest.ImplementsEnumerableGenericType();

            //--------------- Assert -----------------------
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void Collection_ShouldBeEnumerable()
        {
            //--------------- Arrange -------------------
            var src = new Collection<string>(
                new[]
                {
                    "a",
                    "z"
                }
            );

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = src.GetType().ImplementsEnumerableGenericType();

            //--------------- Assert -----------------------
            Expect(result)
                .To.Be.True();
        }

        [TestCase(typeof(string))]
        [TestCase(typeof(int))]
        [TestCase(typeof(object))]
        [TestCase(typeof(TestTypeExtensions))]
        public void ShouldHaveEnumValue_OperatingOnNonEnum_ShouldThrow(Type type)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(
                    () => type.ShouldHaveEnumValue("foo")
                )
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("is not an enum type");
            //--------------- Assert -----------------------
        }

        public enum SomeEnum
        {
            Foo,
            Bar
        }

        [Test]
        public void ShouldHaveEnumValue_GivenNameOnly_WhenEnumDoesNotHaveThatValueByName_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = typeof(SomeEnum);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(
                    () => sut.ShouldHaveEnumValue("Moo")
                )
                .To.Throw<AssertionException>()
                .With.Message
                .Containing($"Could not find value \"Moo\" on enum {sut.PrettyName()}");
            //--------------- Assert -----------------------
        }


        [Test]
        public void ShouldHaveEnumValue_GivenNameOnly_WhenEnumDoesHaveThatValueByName_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = typeof(SomeEnum);
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.ShouldHaveEnumValue("Foo"))
                .Not.To.Throw();
            Expect(() => sut.ShouldHaveEnumValue("Bar"))
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        public enum AnotherEnum
        {
            Foo = 13,
            Bar = 36
        }

        [Test]
        public void ShouldHaveEnumValue_GivenNameAndValue_WhenEnumValueDoesNotMatchRequiredValue_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = typeof(AnotherEnum);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(
                    () => sut.ShouldHaveEnumValue("Foo", 5)
                )
                .To.Throw<AssertionException>()
                .With.Message
                .Containing(
                    $"Could not find enum key \"Foo\" with value \"5\" on enum {sut.PrettyName()}"
                );

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldHaveEnumValue_GivenNameAndValue_WhenEnumValueMatchesRequiredValue_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = typeof(AnotherEnum);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.ShouldHaveEnumValue("Foo", 13))
                .Not.To.Throw();
            Expect(() => sut.ShouldHaveEnumValue("Bar", 36))
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [TestFixture]
        public class HasMethod
        {
            [Test]
            public void ShouldReturnTrueWhenExists()
            {
                // Arrange
                var t = typeof(Dog);
                // Act
                var result = t.HasMethod("Woof");
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseWhenMissing()
            {
                // Arrange
                var t = typeof(Dog);
                // Act
                var result = t.HasMethod("Meow");
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [TestFixture]
            public class GivenReturnType
            {
                [Test]
                public void ShouldReturnTrueWhenMatchingWithNoParameters()
                {
                    // Arrange
                    var t = typeof(Dog);
                    // Act
                    var result = t.HasMethod(
                        "CountLegs",
                        typeof(int)
                    );
                    // Assert
                    Expect(result)
                        .To.Be.True();
                }

                [Test]
                public void ShouldReturnFalseWhenNoMatchBecauseOfParameters()
                {
                    // Arrange
                    var t = typeof(Dog);
                    // Act
                    var result = t.HasMethod(
                        "CountLegs",
                        typeof(int),
                        typeof(string)
                    );
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }

                [Test]
                public void ShouldReturnTrueWHenMatchingParametersAndReturn()
                {
                    // Arrange
                    var t = typeof(Dog);
                    // Act
                    var result = t.HasMethod(
                        "Add",
                        typeof(int),
                        typeof(int),
                        typeof(int)
                    );
                    // Assert
                    Expect(result)
                        .To.Be.True();
                }

                [Test]
                public void ShouldReturnFalseWHenMismatchingParametersAndMatchingReturn()
                {
                    // Arrange
                    var t = typeof(Dog);
                    // Act
                    var result = t.HasMethod(
                        "Add",
                        typeof(int),
                        typeof(int),
                        typeof(string)
                    );
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }
            }

            public class Dog
            {
                public void Woof()
                {
                }

                public int CountLegs()
                {
                    return 4;
                }

                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
        }

        [TestFixture]
        public class IsEnumerable
        {
            [Test]
            public void ShouldReturnTrueForArray()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2,
                    3
                };
                foreach (var item in a)
                {
                    // do nothing - just prove that
                    // this compiles as enumerable
                }

                // Act
                var result = a.GetType().IsEnumerable();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForIEnumerable()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2,
                    3
                }.AsEnumerable<int>();
                var type = a.GetType();
                foreach (var item in a)
                {
                    // do nothing - just prove that
                    // this compiles as enumerable
                }

                // Act
                var result = a.GetType().IsEnumerable();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForHomeGrownEnumerable()
            {
                // Arrange
                var bag = new Bag<int>();
                bag.Add(1);
                bag.Add(2);
                bag.Add(3);
                var expected = new List<int>();
                foreach (var item in bag)
                {
                    expected.Add(item);
                }

                Expect(expected)
                    .To.Equal(
                        new[]
                        {
                            1,
                            2,
                            3
                        }
                    );
                // Act
                var result = bag.GetType().IsEnumerable();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            public class Bag<T>
            {
                private List<T> _store = new();

                public void Add(T value)
                {
                    _store.Add(value);
                }

                public BagEnumerator<T> GetEnumerator()
                {
                    return new BagEnumerator<T>(_store);
                }
            }

            public class BagEnumerator<T>
            {
                private IEnumerator<T> _enumerator;

                public BagEnumerator(IEnumerable<T> data)
                {
                    _enumerator = data.GetEnumerator();
                }

                public void Dispose()
                {
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    _enumerator.Reset();
                }

                public T Current => _enumerator.Current;
            }
        }
    }
}