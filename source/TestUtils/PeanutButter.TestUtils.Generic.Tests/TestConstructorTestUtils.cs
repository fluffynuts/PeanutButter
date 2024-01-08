using System;
using NUnit.Framework;
using PeanutButter.Utils;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestConstructorTestUtils
    {
        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenNonSubstitutableParameter_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed for constructors that only have parameters that can be substituted with NSubstitute.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils
                            .ShouldExpectNonNullParameterFor<ClassWithNonSubstitutableConstructorParameter>(
                                parameterName,
                                typeof(RandomStruct)
                            )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Equal.To(expectedMessage);
            //---------------Test Result -----------------------
        }

        [Test]
        public void WhenNullableParameter_ShouldAllowNonNullParameter()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithNonAbstractConstructorParameter>(
                        parameterName,
                        typeof(int?)
                    )
            ).Not.To.Throw();
            //---------------Test Result -----------------------
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenPrimitiveParameter_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed for constructors that only have parameters that can be substituted with NSubstitute.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithPrimitiveConstructorParameter>(
                            parameterName,
                            typeof(int)
                        )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Equal.To(expectedMessage);
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenMultipleConstructors_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed to test classes with a single constructor.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithMultipleConstructors>(
                            parameterName,
                            typeof(object)
                        )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Equal.To(expectedMessage);
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenInvalidParameterName_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "Cake";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils
                            .ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>(
                                parameterName,
                                typeof(IInterface)
                            )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Containing(parameterName);
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenNoNullArgumentExceptionThrownForParameter_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------
            const string expectedMessage = "Expected: System.ArgumentNullException but was: Null";
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils
                            .ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>(
                                parameterName,
                                typeof(IInterface)
                            )
                ).To.Throw<AssertionException>()
                .With.Message.Equal.To(expectedMessage);
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenNullArgumentExceptionThrownForIncorrectParameter_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------
            const string expectedMessage = "Expected parameter1 to equal WrongParameterName";
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                    () =>
                        ConstructorTestUtils
                            .ShouldExpectNonNullParameterFor<
                                ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions>(
                                parameterName,
                                typeof(SubstitutableAbstractClass)
                            )
                ).To.Throw<AssertionException>()
                .With.Message.Containing(expectedMessage);
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenNullArgumentExceptionThrownForCorrectParameter_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                () =>
                    ConstructorTestUtils
                        .ShouldExpectNonNullParameterFor
                            <ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions>(
                                parameterName,
                                typeof(SubstitutableAbstractClass)
                            )
            ).Not.To.Throw();
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenAbstractParameters_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                () =>
                    ConstructorTestUtils
                        .ShouldExpectNonNullParameterFor
                            <ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions>(
                                parameterName,
                                typeof(SubstitutableAbstractClass)
                            )
            ).Not.To.Throw();
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenInterfaceParameters_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                () =>
                    ConstructorTestUtils
                        .ShouldExpectNonNullParameterFor
                            <ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions>(
                                parameterName,
                                typeof(IInterface)
                            )
            ).Not.To.Throw();
            //---------------Test Result -----------------------
        }

        [Test]
        public void
            CheckForExceptionWhenParameterIsNull_GivenParametersImplementingAnInterface_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Expect(
                () =>
                    ConstructorTestUtils
                        .ShouldExpectNonNullParameterFor
                            <ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions>(
                                parameterName,
                                typeof(SubstitutableClassImplementingAnInterface)
                            )
            ).Not.To.Throw();
            //---------------Test Result -----------------------
        }

        public interface ISomeOtherInterface;

        [Test]
        public void CheckForException_WhenParameterTypeDoesNotMatchExpectedType_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => ConstructorTestUtils
                        .ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>(
                            "parameter1",
                            typeof(ISomeOtherInterface)
                        )
                ).To.Throw<AssertionException>()
                .With.Message.Containing(typeof(ISomeOtherInterface).PrettyName())
                .And
                .Containing(typeof(IInterface).PrettyName());

            //---------------Test Result -----------------------
        }

        public class ClassWithMixedConstructor
        {
            private readonly ISomeOtherInterface _someDependency;

            public ClassWithMixedConstructor(
                Guid id,
                ISomeOtherInterface someDependency
            )
            {
                _someDependency = someDependency ?? throw new ArgumentNullException(nameof(someDependency));
            }
        }

        [Test]
        public void WorkingWithClassWhereOneConstructorParameterIsNotAClass()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ConstructorTestUtils.ShouldExpectNonNullParameterFor<
                ClassWithMixedConstructor
            >("someDependency", typeof(ISomeOtherInterface));

            //---------------Test Result -----------------------
        }


        private class ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions
        {
#pragma warning disable 169
            private readonly SubstitutableAbstractClass _parameter1;
#pragma warning restore 169

            public ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions(
                SubstitutableAbstractClass parameter1
            )
            {
                if (parameter1 == null)
                {
                    throw new ArgumentNullException(nameof(parameter1));
                }
            }
        }

        private class ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions
        {
#pragma warning disable 169
            private readonly SubstitutableAbstractClass _parameter1;
#pragma warning restore 169

            public ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions(
                SubstitutableAbstractClass parameter1
            )
            {
                // ReSharper disable once NotResolvedInText
                if (parameter1 == null)
                {
                    throw new ArgumentNullException("WrongParameterName");
                }
            }
        }

        private class ClassWithConstructorWithSubstitutableParameter
        {
            public ClassWithConstructorWithSubstitutableParameter(IInterface parameter1)
            {
            }
        }


        private class ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions
        {
            public ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions(
                SubstitutableAbstractClass parameter1,
                SubstitutableAbstractClass parameter2
            )
            {
                if (parameter1 == null)
                {
                    throw new ArgumentNullException(nameof(parameter1));
                }

                if (parameter2 == null)
                {
                    throw new ArgumentNullException(nameof(parameter2));
                }
            }
        }

        private class ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions
        {
            public ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions(
                IInterface parameter1,
                IInterface parameter2
            )
            {
                if (parameter1 == null)
                {
                    throw new ArgumentNullException(nameof(parameter1));
                }

                if (parameter2 == null)
                {
                    throw new ArgumentNullException(nameof(parameter2));
                }
            }
        }

        private class
            ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions
        {
            public
                ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions(
                    SubstitutableClassImplementingAnInterface parameter1,
                    SubstitutableClassImplementingAnInterface parameter2
                )
            {
                if (parameter1 == null)
                {
                    throw new ArgumentNullException(nameof(parameter1));
                }

                if (parameter2 == null)
                {
                    throw new ArgumentNullException(nameof(parameter2));
                }
            }
        }

        private class ClassWithMultipleConstructors
        {
            public ClassWithMultipleConstructors(object parameter)
            {
            }

            public ClassWithMultipleConstructors(object parameter1, object parameter2)
            {
            }
        }

        private class ClassWithNonAbstractConstructorParameter
        {
            public ClassWithNonAbstractConstructorParameter(int? parameter)
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException(nameof(parameter));
                }
            }
        }

        private class ClassWithPrimitiveConstructorParameter
        {
            public ClassWithPrimitiveConstructorParameter(int parameter)
            {
            }
        }

        private class ClassWithNonSubstitutableConstructorParameter
        {
            public ClassWithNonSubstitutableConstructorParameter(RandomStruct parameter)
            {
            }
        }
    }

    public abstract class SubstitutableAbstractClass;

    public class SubstitutableClassImplementingAnInterface : IInterface;

    public interface IInterface;

    public struct RandomStruct;
}