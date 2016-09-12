using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.Utils.Windsor.Tests.TestClasses;

namespace PeanutButter.Utils.Windsor.Tests
{
    [TestFixture]
    public class TestWindsorContainerExtensions
    {
        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_GivenNoAssemblies_ShouldThrowArgumentException()
        {
            //---------------Set up test pack-------------------
            var container = Substitute.For<IWindsorContainer>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => container.RegisterAllOneToOneResolutionsAsTransientFrom());

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_ShouldNotRegisterInterfaceWithoutResolution()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);

            //---------------Test Result -----------------------
            Assert.Throws<ComponentNotFoundException>(() => container.Resolve<IInterfaceWithNoResolutions>());
        }

        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_ShouldNotRegisterInterfaceWithMultipleResolutions()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);

            //---------------Test Result -----------------------
            Assert.Throws<ComponentNotFoundException>(() => container.Resolve<IInterfaceWithMultipleResolutions>());
        }

        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_ShouldRegisterOneToOneResolution_IgnoringAbstract()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);

            //---------------Test Result -----------------------
            var result = container.Resolve<IInterfaceWithOneResolution>();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ImplementsInterfaceWithOneResolution>(result);
        }


        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_WhenServiceAlreadyRegistered_ShouldNotAttemptToReRegister()
        {
            //---------------Set up test pack-------------------
            var container = Create();
            container.Register(Component.For<IInterfaceForSingleton>().ImplementedBy<ImplementsInterfaceForSingleton>().LifestyleSingleton());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // Castle Windsor chucks if you try to register the same service twice
            Assert.DoesNotThrow(() => container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly));

            //---------------Test Result -----------------------
        }

        private interface InterfacePart1
        {
        }

        private interface InterfacePart2
        {
        }

        private class Implementation: InterfacePart1, InterfacePart2
        {
        }


        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_WhenImplementationAlreadyRegisteredForDifferentService_ShouldNotAttemptToReRegister()
        {
            //---------------Set up test pack-------------------
            var container = Create();
            container.RegisterTransient<InterfacePart1, Implementation>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // Castle Windsor chucks if you try to register the same service twice
            Assert.DoesNotThrow(() => container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly));

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllOneToOneResolutionsAsTransientFrom_ShouldNotRegisterInterfacesImplementingInterfaces()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);

            //---------------Test Result -----------------------
            Assert.Throws<ComponentNotFoundException>(() => container.Resolve<IInterfaceWithOneResolutionInheritor>());
        }

        [Test]
        public void RegisterAllOneToOneResulitionsAsTransientFrom_ShouldNotRegisterAbstractClasses()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);

            //---------------Test Result -----------------------
            Assert.Throws<ComponentNotFoundException>(() => container.Resolve<IInterfaceForAbstractClass>());

        }

        [Test]
        public void RegisterAllMvcControllersFrom_GivenNoAssemblies_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => container.RegisterAllMvcControllersFrom());

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllMvcControllersFrom_GivenAssemblyWithNoControllers_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => container.RegisterAllMvcControllersFrom(typeof(WindsorContainerExtensions).Assembly));

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllMvcControllersFrom_GivenAssemblyContainingControllerClasses_ShouldRegisterThem()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => container.RegisterAllMvcControllersFrom(GetType().Assembly));

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<HomeController>(container.Resolve<HomeController>());
            Assert.IsInstanceOf<AccountController>(container.Resolve<AccountController>());
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenNoAssemblies_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => container.RegisterAllApiControllersFrom());

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenAssemblyWithNoControllers_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => container.RegisterAllApiControllersFrom(typeof(WindsorContainerExtensions).Assembly));

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenAssemblyContainingControllerClasses_ShouldRegisterThem()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => container.RegisterAllApiControllersFrom(GetType().Assembly));

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<SomeApiController>(container.Resolve<SomeApiController>());
        }


        [Test]
        public void RegisterSingleton_Generic_ShouldRegisterServiceAsSingleton()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterSingleton<ISingletonService, SingletonService>();
            var result1 = container.Resolve<ISingletonService>();
            var result2 = container.Resolve<ISingletonService>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsInstanceOf<SingletonService>(result1);
            Assert.IsInstanceOf<SingletonService>(result2);
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void RegisterSingleton_GivenTwoTypes_ShouldRegisterServiceAsSingleton()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterSingleton(typeof(ISingletonService), typeof(SingletonService));
            var result1 = container.Resolve<ISingletonService>();
            var result2 = container.Resolve<ISingletonService>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsInstanceOf<SingletonService>(result1);
            Assert.IsInstanceOf<SingletonService>(result2);
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void RegisterTransient_ShouldRegisterServiceAsTransient()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterTransient<IInterfaceWithMultipleResolutions, ImplementsInterfaceWithMultipleResolutions1>();
            var result1 = container.Resolve<IInterfaceWithMultipleResolutions>();
            var result2 = container.Resolve<IInterfaceWithMultipleResolutions>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsInstanceOf<ImplementsInterfaceWithMultipleResolutions1>(result1);
            Assert.IsInstanceOf<ImplementsInterfaceWithMultipleResolutions1>(result2);
            Assert.AreNotEqual(result1, result2);
        }

        [Test]
        public void RegisterPerWebRequest_ShouldRegisterPerWebRequest()
        {
            // TODO: actually prove PerWebRequest lifestyle...
            //---------------Set up test pack-------------------
            var container = Substitute.For<IWindsorContainer>();
            var registrations = new List<IRegistration>();
            container.Register(Arg.Any<IRegistration>())
                .Returns(ci =>
                {
                    registrations.Add((ci.Args()[0] as IRegistration[])[0]);
                    return container;
                });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterPerWebRequest<ISingletonService, SingletonService>();

            //---------------Test Result -----------------------
            var registration = registrations.Single();
            Assert.IsNotNull(registration);
            var lifestyle = registration.GetOrDefault<Castle.MicroKernel.Registration.Lifestyle.LifestyleGroup<ISingletonService>>("LifeStyle");
            //lifestyle.PerWebRequest.
            Assert.IsNotNull(lifestyle);
        }

        private interface IDependencyForSingleInstance
        {
        }

        private class DependencyForSingleInstance : IDependencyForSingleInstance
        {
        }

        [Test]
        public void RegisterInstance_ShouldRegisterSingleProvidedInstanceForResolution()
        {
            //---------------Set up test pack-------------------
            var container = new WindsorContainer();
            var instance = new DependencyForSingleInstance();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterInstance<IDependencyForSingleInstance>(instance);
            var result1 = container.Resolve<IDependencyForSingleInstance>();
            var result2 = container.Resolve<IDependencyForSingleInstance>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result1, instance);
            Assert.AreEqual(result2, instance);

        }



        private static IWindsorContainer Create()
        {
            return new WindsorContainer();
        }
    }
}
