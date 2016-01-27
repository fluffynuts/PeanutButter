using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestConstructorTestUtils
    {
        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenNonSubstitutableParameter_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed for constructors that only have parameters that can be substituted with NSubstitute.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithNonSubstitutableConstructorParameter>(
                        parameterName, typeof(RandomStruct)));
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenNonAbstractParameter_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed for constructors that only have parameters that can be substituted with NSubstitute.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithNonAbstractConstructorParameter>(
                        parameterName, typeof(int?)));
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedMessage, exception.Message);
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
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithPrimitiveConstructorParameter>(
                        parameterName, typeof(int)));
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenMultipleConstructors_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter";
            const string expectedMessage =
                "This utility is designed to test classes with a single constructor.";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithMultipleConstructors>(
                        parameterName, typeof(object)));
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenInvalidParameterName_ShouldThrowInvalidOperationException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "Cake";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>(
                        parameterName, typeof(IInterface)));
            //---------------Test Result -----------------------
            StringAssert.Contains(parameterName, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenNoNullArgumentExceptionThrownForParameter_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------
            const string expectedMessage = "Expected: System.ArgumentNullException but was: Null";
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<AssertionException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>(
                        parameterName, typeof(IInterface)));
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenNullArgumentExceptionThrownForIncorrectParameter_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------
            const string expectedMessage = "Expected string length";
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var exception =
                Assert.Throws<AssertionException>(
                    () =>
                    ConstructorTestUtils.ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions>(
                        parameterName, typeof(SubstitutableAbstractClass)));
            //---------------Test Result -----------------------
            StringAssert.Contains(expectedMessage, exception.Message);
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenNullArgumentExceptionThrownForCorrectParameter_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter1";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(
                () =>
                ConstructorTestUtils
                    .ShouldExpectNonNullParameterFor
                    <ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions>(
                        parameterName, typeof(SubstitutableAbstractClass)));
            //---------------Test Result -----------------------
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenAbstractParameters_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(
                () =>
                ConstructorTestUtils
                    .ShouldExpectNonNullParameterFor
                    <ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions>(
                        parameterName, typeof(SubstitutableAbstractClass)));
            //---------------Test Result -----------------------
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenInterfaceParameters_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(
                () =>
                ConstructorTestUtils
                    .ShouldExpectNonNullParameterFor
                    <ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions>(
                        parameterName, typeof(IInterface)));
            //---------------Test Result -----------------------
        }

        [Test]
        public void CheckForExceptionWhenParameterIsNull_GivenParametersImplementingAnInterface_AndNullParameterIsNotTheFirstParameter_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            const string parameterName = "parameter2";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(
                () =>
                ConstructorTestUtils
                    .ShouldExpectNonNullParameterFor
                    <ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions>(
                        parameterName, typeof(SubstitutableClassImplementingAnInterface )));
            //---------------Test Result -----------------------
        }

        public interface ISomeOtherInterface
        {
        }

        [Test]
        public void CheckForException_WhenParameterTypeDoesNotMatchExpectedType_ShouldThrowAssertionException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => ConstructorTestUtils
                .ShouldExpectNonNullParameterFor<ClassWithConstructorWithSubstitutableParameter>("parameter1", typeof(ISomeOtherInterface)));

            //---------------Test Result -----------------------
            StringAssert.Contains(typeof(ISomeOtherInterface).PrettyName(), ex.Message);
            StringAssert.Contains(typeof(IInterface).PrettyName(), ex.Message);
        }

        class ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions
        {
#pragma warning disable 169
            private readonly SubstitutableAbstractClass _parameter1;
#pragma warning restore 169

            public ClassWithConstructorWithSubstitutableParameterThatThrowsCorrectExceptions(SubstitutableAbstractClass parameter1)
            {
                if (parameter1 == null) throw new ArgumentNullException("parameter1");
            }
        }

        class ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions
        {
#pragma warning disable 169
            private readonly SubstitutableAbstractClass _parameter1;
#pragma warning restore 169

            public ClassWithConstructorWithSubstitutableParameterThatThrowsIncorrectExceptions(SubstitutableAbstractClass parameter1)
            {
                if (parameter1 == null) throw new ArgumentNullException("WrongParameterName");
            }
        }

        class ClassWithConstructorWithSubstitutableParameter
        {
            public ClassWithConstructorWithSubstitutableParameter(IInterface parameter1)
            {
            }
        }


        class ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions
        {
            public ClassWithConstructorWithMultipleSubstitutableAbstractParametersThatThrowsCorrectExceptions(SubstitutableAbstractClass parameter1, SubstitutableAbstractClass parameter2)
            {
                if (parameter1 == null) throw new ArgumentNullException("parameter1");
                if (parameter2 == null) throw new ArgumentNullException("parameter2");
            }
        }

        class ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions
        {
            public ClassWithConstructorWithMultipleSubstitutableInterfaceParametersThatThrowsCorrectExceptions(IInterface parameter1, IInterface parameter2)
            {
                if (parameter1 == null) throw new ArgumentNullException("parameter1");
                if (parameter2 == null) throw new ArgumentNullException("parameter2");
            }
        }

        class ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions
        {
            public ClassWithConstructorWithMultipleSubstitutableParametersImplementingAnInterfaceThatThrowsCorrectExceptions(SubstitutableClassImplementingAnInterface parameter1, SubstitutableClassImplementingAnInterface parameter2)
            {
                if (parameter1 == null) throw new ArgumentNullException("parameter1");
                if (parameter2 == null) throw new ArgumentNullException("parameter2");
            }
        }

        class ClassWithMultipleConstructors
        {
            public ClassWithMultipleConstructors(object parameter)
            {
            }

            public ClassWithMultipleConstructors(object parameter1, object parameter2)
            {
            }
        }

        class ClassWithNonAbstractConstructorParameter
        {
            public ClassWithNonAbstractConstructorParameter(int? parameter)
            {
            }
        }

        class ClassWithPrimitiveConstructorParameter
        {
            public ClassWithPrimitiveConstructorParameter(int parameter)
            {
            }
        }

        class ClassWithNonSubstitutableConstructorParameter
        {
            public ClassWithNonSubstitutableConstructorParameter(RandomStruct parameter)
            {
            }
        }
    }

    public abstract class SubstitutableAbstractClass
    {}

    public class SubstitutableClassImplementingAnInterface : IInterface
    {}

    public interface IInterface
    {}

    public struct RandomStruct
    {}
}
