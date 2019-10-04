using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.Utils.Windsor.Tests.TestClasses;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException

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
            Expect(
                    () => container.RegisterAllOneToOneResolutionsAsTransientFrom()
                )
                .To.Throw<ArgumentException>();
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
            Expect(
                    () => container.Resolve<IInterfaceWithNoResolutions>()
                )
                .To.Throw<ComponentNotFoundException>();
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
            Expect(() => container.Resolve<IInterfaceWithMultipleResolutions>())
                .To.Throw<ComponentNotFoundException>();
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
            Expect(result).Not.To.Be.Null();
            Expect(result).To.Be.An.Instance.Of<ImplementsInterfaceWithOneResolution>();
        }


        [Test]
        public void
            RegisterAllOneToOneResolutionsAsTransientFrom_WhenServiceAlreadyRegistered_ShouldNotAttemptToReRegister()
        {
            //---------------Set up test pack-------------------
            var container = Create();
            container.Register(Component.For<IInterfaceForSingleton>()
                .ImplementedBy<ImplementsInterfaceForSingleton>()
                .LifestyleSingleton());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // Castle Windsor chucks if you try to register the same service twice
            Expect(
                    () => container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly)
                )
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        private interface InterfacePart1
        {
        }

        private interface InterfacePart2
        {
        }

        private class Implementation : InterfacePart1, InterfacePart2
        {
        }


        [Test]
        public void
            RegisterAllOneToOneResolutionsAsTransientFrom_WhenImplementationAlreadyRegisteredForDifferentService_ShouldNotAttemptToReRegister()
        {
            //---------------Set up test pack-------------------
            var container = Create();
            container.RegisterTransient<InterfacePart1, Implementation>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly)
                )
                .Not.To.Throw();
            var result1 = container.Resolve<InterfacePart1>();
            var result2 = container.Resolve<InterfacePart2>();

            //---------------Test Result -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<Implementation>();
            Expect(result2).To.Be.An.Instance.Of<Implementation>();
        }

        [Test]
        public void
            RegisterAllOneToOneResolutionsAsTransientExcept_GivenIgnoreListAndAssemblyList_ShouldIgnoreThoseTypesAsServices()
        {
            //--------------- Arrange -------------------
            var container = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientExcept(
                new[] {typeof(InterfacePart1)},
                new[] {GetType().Assembly}
            );

            //--------------- Assert -----------------------
            var result1 = container.Resolve<InterfacePart2>();
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<Implementation>();
            Expect(() => container.Resolve<InterfacePart1>())
                .To.Throw<ComponentNotFoundException>();
        }

        [Test]
        public void
            RegisterAllOneToOneResolutionsAsTransientExcept_GivenIgnoreListAndAssemblyList_ShouldIgnoreThoseTypesAsImplementationsToo()
        {
            //--------------- Arrange -------------------
            var container = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            container.RegisterAllOneToOneResolutionsAsTransientExcept(
                new[] {typeof(Implementation)},
                new[] {GetType().Assembly}
            );

            //--------------- Assert -----------------------
            Expect(() => container.Resolve<InterfacePart2>())
                .To.Throw<ComponentNotFoundException>();
            Expect(() => container.Resolve<InterfacePart1>())
                .To.Throw<ComponentNotFoundException>();
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
            Expect(
                    () => container.Resolve<IInterfaceWithOneResolutionInheritor>()
                )
                .To.Throw<ComponentNotFoundException>();
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
            Expect(
                    () => container.Resolve<IInterfaceForAbstractClass>()
                )
                .To.Throw<ComponentNotFoundException>();
        }

        [Test]
        public void RegisterAllMvcControllersFrom_GivenNoAssemblies_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllMvcControllersFrom()
                )
                .To.Throw<ArgumentException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllMvcControllersFrom_GivenAssemblyWithNoControllers_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllMvcControllersFrom(typeof(WindsorContainerExtensions).Assembly)
                )
                .Not.To.Throw();

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
            Expect(container.Resolve<HomeController>())
                .To.Be.An.Instance.Of<HomeController>();
            Expect(container.Resolve<AccountController>())
                .To.Be.An.Instance.Of<AccountController>();
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenNoAssemblies_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllApiControllersFrom()
                )
                .To.Throw<ArgumentException>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenAssemblyWithNoControllers_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllApiControllersFrom(typeof(WindsorContainerExtensions).Assembly)
                )
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void RegisterAllApiControllersFrom_GivenAssemblyContainingControllerClasses_ShouldRegisterThem()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(
                    () => container.RegisterAllApiControllersFrom(GetType().Assembly)
                )
                .Not.To.Throw();

            //---------------Test Result -----------------------
            Expect(container.Resolve<SomeApiController>())
                .To.Be.An.Instance.Of<SomeApiController>();
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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<SingletonService>();
            Expect(result2).To.Be.An.Instance.Of<SingletonService>();
            Expect(result1).To.Equal(result2);
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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<SingletonService>();
            Expect(result2).To.Be.An.Instance.Of<SingletonService>();
            Expect(result1).To.Equal(result2);
        }

        [Test]
        public void RegisterTransient_ShouldRegisterServiceAsTransient()
        {
            //---------------Set up test pack-------------------
            var container = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container
                .RegisterTransient<IInterfaceWithMultipleResolutions, ImplementsInterfaceWithMultipleResolutions1>();
            var result1 = container.Resolve<IInterfaceWithMultipleResolutions>();
            var result2 = container.Resolve<IInterfaceWithMultipleResolutions>();

            //---------------Test Result -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<ImplementsInterfaceWithMultipleResolutions1>();
            Expect(result2).To.Be.An.Instance.Of<ImplementsInterfaceWithMultipleResolutions1>();
            Expect(result1).Not.To.Equal(result2);
        }

        [Test]
        public void RegisterPerWebRequest_ShouldRegisterPerWebRequest()
        {
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
            Expect(registration).Not.To.Be.Null();

            var lifestyle =
                registration.GetOrDefault<Castle.MicroKernel.Registration.Lifestyle.LifestyleGroup<ISingletonService>>(
                    "LifeStyle");
            Expect(lifestyle).Not.To.Be.Null();
            // TODO: actually prove PerWebRequest lifestyle...
        }

        private interface IDependencyForSingleInstanceBase
        {
        }

        private interface IDependencyForSingleInstance : IDependencyForSingleInstanceBase
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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Equal(result2);
            Expect(result1).To.Equal(instance);
            Expect(result2).To.Equal(instance);
        }

        [Test]
        public void
            RegisterInstance_ShouldRegisterSingleProvidedInstanceForResolutionOnMultipleAttemptsWithDifferentInterface()
        {
            //---------------Set up test pack-------------------
            var container = new WindsorContainer();
            var instance = new DependencyForSingleInstance();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterInstance<IDependencyForSingleInstance>(instance);
            container.RegisterInstance<IDependencyForSingleInstanceBase>(instance);
            var result1 = container.Resolve<IDependencyForSingleInstance>();
            var result2 = container.Resolve<IDependencyForSingleInstanceBase>();

            //---------------Test Result -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Equal(result2 as IDependencyForSingleInstance);
            Expect(result1).To.Equal(instance);
            Expect(result2).To.Equal(instance);
        }

        [Test]
        public void RegisterInstance_ShouldNotThrowIfTryingToRegisterTwiceForTheSameInterfaceAndImplementation()
        {
            //---------------Set up test pack-------------------
            var container = new WindsorContainer();
            var instance = new DependencyForSingleInstance();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            container.RegisterInstance<IDependencyForSingleInstance>(instance);

            Expect(() =>
                    container.RegisterInstance<IDependencyForSingleInstance>(instance)
                )
                .To.Throw<ComponentRegistrationException>();
            //---------------Test Result -----------------------
        }


        private static IWindsorContainer Create()
        {
            return new WindsorContainer();
        }
    }
}