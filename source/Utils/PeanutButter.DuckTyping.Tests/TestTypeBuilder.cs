using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestTypeBuilder
    {
        public interface ISample1
        {
            string Name { get; }
        }

        [Test]
        public void BuildFor_GivenInterfaceWithOneProperty_ShouldReturnTypeImplementingThatInterface()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<ISample1>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Assert.IsInstanceOf<ISample1>(instance);
        }

        private readonly MethodInfo _genericCreate = typeof(Activator)
                                                    .GetMethods()
                                                    .First(mi => mi.Name == "CreateInstance" &&
                                                                 mi.IsGenericMethod &&
                                                                 mi.GetParameters().Length == 0);

        private object CreateInstanceOf(Type type)
        {
            var specificCreate = _genericCreate.MakeGenericMethod(type);
            return specificCreate.Invoke(null, new object[] { });
        }

        private ITypeMaker Create()
        {
            return new TypeMaker();
        }

    }
}
