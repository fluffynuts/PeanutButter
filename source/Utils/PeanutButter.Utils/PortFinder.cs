using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Utility class to find open ports for binding to
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class PortFinder
    {
        /// <summary>
        /// The lowest port number that is considered by default
        /// - 1024 because that doesn't require privilege escalation
        /// </summary>
        public const int MIN_PORT = 1024;

        /// <summary>
        /// The highest port number considered by default
        /// </summary>
        public const int MAX_PORT = 65535;

        /// <summary>
        /// Attempts to find a random unbound port on the loopback device (localhost)
        /// within the range 1024-65535
        /// </summary>
        /// <returns></returns>
        public static int FindOpenPort()
        {
            return FindOpenPort(IPAddress.Loopback);
        }

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the range 1024-65535
        /// </summary>
        /// <param name="forAddress"></param>
        /// <returns></returns>
        public static int FindOpenPort(IPAddress forAddress)
        {
            return FindOpenPort(forAddress, MIN_PORT, MAX_PORT);
        }

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the provided range
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int FindOpenPort(
            IPAddress forAddress,
            int min,
            int max
        )
        {
            return FindOpenPort(
                forAddress,
                min,
                max,
                NullLogger
            );
        }

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the provided range
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="attemptLogger">Action to invoke to log port finder attempts</param>
        /// <returns></returns>
        public static int FindOpenPort(
            IPAddress forAddress,
            int min,
            int max,
            Action<string> attemptLogger
        )
        {
            var tried = new HashSet<int>();
            return FindOpenPort(
                forAddress,
                min,
                max,
                (minPort, maxPort, _) => Next(minPort, maxPort, tried),
                attemptLogger
            );
        }

        private static readonly HashSet<int> Used = new();

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the provided range using the provided port attempt generator
        /// The portAttemptGenerator callback is called with 3 arguments:
        /// - minimum port
        /// - maximum port
        /// - last attempt
        /// and should return a new value to test
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="portAttemptGenerator"></param>
        /// <returns></returns>
        public static int FindOpenPort(
            IPAddress forAddress,
            int min,
            int max,
            Func<int, int, int, int> portAttemptGenerator
        )
        {
            return FindOpenPort(
                forAddress,
                min,
                max,
                portAttemptGenerator,
                NullLogger
            );
        }

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the provided range using the provided port attempt generator
        /// The portAttemptGenerator callback is called with 3 arguments:
        /// - minimum port
        /// - maximum port
        /// - last attempt
        /// and should return a new value to test
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="portAttemptGenerator"></param>
        /// <param name="attemptLogger">Action to invoke to log port finder attempts</param>
        /// <returns></returns>
        public static int FindOpenPort(
            IPAddress forAddress,
            int min,
            int max,
            Func<int, int, int, int> portAttemptGenerator,
            Action<string> attemptLogger
        )
        {
            var maxTries = Math.Abs(max - min);
            var test = 0;
            for (var i = 0; i < maxTries; i++)
            {
                lock (Used)
                {
                    do
                    {
                        test = portAttemptGenerator(min, max, test);
                    } while (Used.Contains(test));

                    Used.Add(test);
                }

                attemptLogger?.Invoke($"Testing if port {test} is already bound on {forAddress}");
                if (PortIsActivelyInUse(forAddress, test))
                {
                    continue;
                }

                attemptLogger?.Invoke($"Looks like {forAddress}:{test} is not bound");
                return test;
            }

            throw new UnableToFindOpenPortException(
                forAddress,
                min,
                max
            );
        }

        private static void NullLogger(string obj)
        {
        }

        /// <summary>
        /// Tests if the provided port is currently bound on the loopback device (localhost)
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsActivelyInUse(int port)
        {
            return PortIsActivelyInUse(IPAddress.Loopback, port);
        }

        /// <summary>
        /// Tests if the provided port is currently bound on the provided device
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsActivelyInUse(IPAddress forAddress, int port)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(new IPEndPoint(forAddress, port));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static int Next(
            int min,
            int max,
            HashSet<int> tried
        )
        {
            if (min > max)
            {
                var swap = min;
                min = max;
                max = swap;
            }

            int result;
            lock (Used)
            {
                do
                {
                    result = RandomNumber.Next(min, max);
                } while (tried.Contains(result) || Used.Contains(result));
            }

            return result;
        }
    }
}