using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestTypeExtensions
    {
        public class HasNoActions { }

        public class HasMethodCalledFoo
        {
            public int Foo()
            {
                return -1;
            }
        }

        public class HasActionCalledFoo
        {
            public void Foo() { }
        }

        [Test]
        public void HasActionWithName_GivenTypeWithoutAction_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasNoActions);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.HasActionMethodWithName(RandomValueGen.GetRandomAlphaString(2, 10));

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
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
            Assert.IsFalse(result);
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
            Assert.IsTrue(result);
        }
        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithoutAction_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasNoActions);
            var actionName = RandomValueGen.GetRandomAlphaString(2, 10);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() =>
                    t.ShouldHaveActionMethodWithName(actionName));

            //---------------Test Result -----------------------
            Assert.AreEqual("Expected to find method '" + actionName + "' on type 'HasNoActions' but didn't.", ex.Message);
        }

        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithMethodReturningValueByThatName_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasMethodCalledFoo);
            var actionName = "Foo";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() =>
                    t.ShouldHaveActionMethodWithName(actionName));

            //---------------Test Result -----------------------
            Assert.AreEqual("Expected to find method '" + actionName + "' on type 'HasMethodCalledFoo' but didn't.", ex.Message);
        }

        [Test]
        public void ShouldHaveActionWithName_GivenTypeWithActionByThatName_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(HasActionCalledFoo);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => t.ShouldHaveActionMethodWithName("Foo"));

            //---------------Test Result -----------------------
        }

        public interface IInterface { }
        public class DoesNotImplement { };
        public class DoesImplement : IInterface { };

        [Test]
        public void ShouldImplement_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(t.ShouldImplement<IInterface>);

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesNotImplement should implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldImplement_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplement).ShouldImplement<IInterface>());

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => t.ShouldImplement(typeof(IInterface)));

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesNotImplement should implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplement).ShouldImplement(typeof(IInterface)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplement_WhenTypeDoesNotImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(t.ShouldNotImplement<IInterface>);

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplement_WhenTypeDoesImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => typeof(DoesImplement).ShouldNotImplement<IInterface>());

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesImplement should not implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldNotImplementWithArguments_WhenTypeDoesNotImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => t.ShouldNotImplement(typeof(IInterface)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => typeof(DoesImplement).ShouldNotImplement(typeof(IInterface)));

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesImplement should not implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldIInheritFrom_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(t.ShouldInheritFrom<IInterface>);

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesNotImplement should implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldInheritFrom_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplement).ShouldInheritFrom<IInterface>());

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
            Assert.AreEqual("DoesImplement", result);
        }

        public class GenericType<T> { }

        [Test]
        public void PrettyName_GIvenGenericType_ShouldReturnReadableGenericName()
        {
            //---------------Set up test pack-------------------
            var t = typeof(GenericType<string>);           
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.PrettyName();

            //---------------Test Result -----------------------
            Assert.AreEqual("TestTypeExtensions+GenericType<String>", result);
        }

        public class CanHaveNullParameter
        {
            public CanHaveNullParameter(string arg)
            {
            }
        }

        [Test]
        public void ShouldThrowWhenConstructorParameterIsNull_WhenGivenTypeWithParameterSetToNullAndConstructorDoesNotThrow_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CanHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(string)));

            //---------------Test Result -----------------------
        }

        public class CannotHaveNullParameter
        {
            public CannotHaveNullParameter(string arg)
            {
                if (arg == null) throw new ArgumentNullException("arg");
            }
        }

        [Test]
        public void ShouldThrowWhenConstructorParameterIsNull_GivenTypeWhichChecksForNull_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CannotHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(string)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldThrowWhenConstructorParameterIsNull_GivenTypeWhichChecksForNull_ShouldThrowIfParameterTypeDoesNotMatch()
        {
            //---------------Set up test pack-------------------
            var t = typeof(CannotHaveNullParameter);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => t.ShouldThrowWhenConstructorParameterIsNull("arg", typeof(int)));

            //---------------Test Result -----------------------
            Assert.AreEqual("Parameter arg is expected to have type: 'Int32' but actually has type: 'String'", ex.Message);
        }

    }
}
