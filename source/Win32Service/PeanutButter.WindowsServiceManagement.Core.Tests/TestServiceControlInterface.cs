using System;
using System.Linq;
using NExpect;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement.Exceptions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.WindowsServiceManagement.Core.Tests
{
    [TestFixture]
    [Explicit($"Requires running on windows, with {ServiceName} installed")]
    public class TestServiceControlInterface
    {
        private const string ServiceName = "rabbitmq";
        private const string DisplayName = "RabbitMQ";

        [TestFixture]
        public class WhenGivenUnknownServiceName
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var sut = Create();
                var serviceName = GetRandomString(20, 30);
                // Act
                Expect(() => sut.QueryConfiguration(serviceName))
                    .To.Throw<ServiceNotInstalledException>()
                    .With.Message.Containing("specified service does not exist");
                // Assert
            }
        }

        [TestFixture]
        public class FindServiceByPid
        {
            [Test]
            [Explicit("requires a running service on windows")]
            public void ShouldBeAbleToFindRunningService()
            {
                // Arrange
                var expected = "RabbitMQ";
                var pid = 5932; // this must be manually set per run - use sc queryex to find the pid
                var sut = Create();
                // Act
                var serviceName = sut.FindServiceByPid(pid);
                // Assert
                Expect(serviceName)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Explicit($"Requires running on windows, with {ServiceName} installed")]
        public class QueryEx
        {
            [Test]
            public void ShouldReturnServiceName()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.SERVICE_NAME)
                    .With.Value("rabbitmq");
            }

            [Test]
            public void ShouldReturnType()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.TYPE)
                    .With.Value("10  WIN32_OWN_PROCESS");
            }

            [Test]
            public void ShouldReturnServiceState()
            {
                // Arrange
                var sut = Create();
                
                // Act
                var result = sut.QueryEx(ServiceName);
                
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.STATE)
                    .With.Value("4  RUNNING (STOPPABLE, NOT_PAUSABLE, ACCEPTS_SHUTDOWN)");
            }

            [TestCase(ServiceControlKeys.WIN32_EXIT_CODE)]
            [TestCase(ServiceControlKeys.SERVICE_EXIT_CODE)]
            [TestCase(ServiceControlKeys.WAIT_HINT)]
            [TestCase(ServiceControlKeys.PROCESS_ID)]
            [TestCase(ServiceControlKeys.FLAGS)]
            public void ShouldContainKey_(string expected)
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(expected);
            }
        }

        [TestFixture]
        public class QueryConfiguration
        {
            [Test]
            public void ShouldReturnType()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.TYPE)
                    .With.Value("10  WIN32_OWN_PROCESS");
            }

            [Test]
            public void ShouldIncludeStartType()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.START_TYPE)
                    .With.Value("2   AUTO_START");
            }

            [Test]
            public void ShouldIncludeErrorControl()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.ERROR_CONTROL)
                    .With.Value("1   NORMAL");
            }

            [Test]
            public void ShouldIncludeBinPath()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.BINARY_PATH_NAME);
                Expect(result[ServiceControlKeys.BINARY_PATH_NAME])
                    .To.Exist();
            }

            [Test]
            public void ShouldIncludeDisplayName()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.DISPLAY_NAME)
                    .With.Value(DisplayName);
            }

            [TestCase(ServiceControlKeys.LOAD_ORDER_GROUP)]
            [TestCase(ServiceControlKeys.TAG)]
            [TestCase(ServiceControlKeys.DEPENDENCIES)]
            [TestCase(ServiceControlKeys.SERVICE_START_NAME)]
            public void ShouldInclude_(string expected)
            {
                // Arrange
                var sut = Create();
                
                // Act
                var result = sut.QueryConfiguration(ServiceName);
                
                // Assert
                Expect(result)
                    .To.Contain.Key(expected);
            }
        }

        [TestFixture]
        public class QueryAll
        {
            [Test]
            public void ShouldContainAllKeys()
            {
                // Arrange
                var keys = typeof(ServiceControlKeys).GetAllConstantValues<string>();
                var sut = Create();
                // Act
                var result = sut.QueryAll(ServiceName);
                // Assert
                Expect(result.Keys)
                    .To.Contain.All.Of(keys);
            }
        }

        [TestFixture]
        [Explicit("machine-dependent")]
        public class ListServices
        {
            [Test]
            public void ShouldReturnAllNamesOfServices()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.ListAllServices()
                    .ToArray();
                // Assert
                Expect(result)
                    .To.Contain("RabbitMQ");
                Expect(result)
                    .To.Contain("MySQL57");
                    
            }
        }
        
        private static IServiceControlInterface Create()
        {
            return new ServiceControlInterface();
        }
    }
}