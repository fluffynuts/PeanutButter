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
        public void ShouldImplement_WhenTypeDoesImplementOtherInterface_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplement).ShouldImplement<IInterface>());

            //---------------Test Result -----------------------
        }

        public class DoesImplementDerivative: DoesImplement
        {
        }

        [Test]
        public void ShouldImplement_WhenRequestedTypeIsAClass_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => typeof(DoesImplementDerivative).ShouldImplement<DoesImplement>());

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldInheritFrom_WhenRequestedTypeIsABaseClass_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplementDerivative).ShouldInheritFrom<DoesImplement>());

            //---------------Test Result -----------------------
        }


        [Test]
        public void ShouldInheritFrom_WhenRequestedTypeIsAnInterface_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => typeof(DoesImplementDerivative).ShouldInheritFrom<Tests.IInterface>());

            //---------------Test Result -----------------------
        }



        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesNotImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var t = typeof(DoesNotImplement);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => t.ShouldBeAssignableFrom(typeof(IInterface)));

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesNotImplement should implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => typeof(DoesImplement).ShouldBeAssignableFrom(typeof(IInterface)));

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
            Assert.DoesNotThrow(() => t.ShouldNotBeAssignableFrom(typeof(IInterface)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotImplementWithArguments_WhenTypeDoesImplementOtherType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => typeof(DoesImplement).ShouldNotBeAssignableFrom(typeof(IInterface)));

            //---------------Test Result -----------------------
            Assert.AreEqual("DoesImplement should not implement IInterface", ex.Message);
        }

        [Test]
        public void ShouldIImplementm_WhenTypeDoesNotImplementOtherType_ShouldThrow()
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

        [Test]
        public void Method_WhenCondition_ShouldExpectedBehaviour()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
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

        public abstract class AbstractThing
        {
        }

        [Test]
        public void ShouldBeAbstract_GivenAbstractType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (AbstractThing);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldBeAbstract());

            //---------------Test Result -----------------------
        }

        public class NotAnAbstractThing: AbstractThing
        {
        }

        [Test]
        public void ShouldBeAbstract_GivenNonAbstractType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (NotAnAbstractThing);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => sut.ShouldBeAbstract());

            //---------------Test Result -----------------------
            StringAssert.Contains(sut.Name, ex.Message);
            StringAssert.Contains("should be abstract", ex.Message);
        }

        public class ClassWithAProperty
        {
            public int IntProperty { get; set; }
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameOnly_WhenPropertyExists_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldHaveProperty("IntProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenInvalidNameOnly_WhenPropertyExists_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldHaveProperty("IntProperty1"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameAndType_WhenPropertyExistsWithExpectedType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldHaveProperty("IntProperty", typeof(int)));
            Assert.DoesNotThrow(() => sut.ShouldHaveProperty<int>("IntProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveProperty_GivenValidNameAndInvalidType_WhenPropertyExistsWithDifferentType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldHaveProperty("IntProperty", typeof(string)));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveProperty<string>("IntProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenInvalidNameOnly_WhenPropertyDoesNotExist_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldNotHaveProperty("StringProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenNameOnly_WhenPropertyExists_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldNotHaveProperty("IntProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenNameAndType_WhenPropertyExistsWithType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldNotHaveProperty("IntProperty", typeof(int)));
            Assert.Throws<AssertionException>(() => sut.ShouldNotHaveProperty<int>("IntProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotHaveProperty_GivenValidNameAndType_WhenPropertyExistsWithDifferentType_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ClassWithAProperty);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldNotHaveProperty("IntProperty", typeof(string)));
            Assert.DoesNotThrow(() => sut.ShouldNotHaveProperty<string>("IntProperty"));

            //---------------Test Result -----------------------
        }

        public class ReadOnlyPropertyTestClass
        {
            public int ReadOnlyProperty { get; private set; }
            public int ReadWriteProperty { get; set; }
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsReadOnly_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty"));
            Assert.DoesNotThrow(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty", typeof(int)));
            Assert.DoesNotThrow(() => sut.ShouldHaveReadOnlyProperty<int>("ReadOnlyProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsNotReadOnly_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty("ReadWriteProperty"));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty("ReadWriteProperty", typeof(int)));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty<int>("ReadWriteProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyDoesNotExistByName_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty("SomeProperty"));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty("SomeProperty", typeof(int)));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty<int>("SomeProperty"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveReadOnlyProperty_WhenPropertyExistsAndIsReadOnlyButHasDifferentType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ReadOnlyPropertyTestClass);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty("ReadOnlyProperty", typeof(string)));
            Assert.Throws<AssertionException>(() => sut.ShouldHaveReadOnlyProperty<bool>("ReadOnlyProperty"));

            //---------------Test Result -----------------------
        }


    }
}
