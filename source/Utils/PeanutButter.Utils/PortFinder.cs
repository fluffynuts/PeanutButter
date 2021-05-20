using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Utility class to find open ports for binding to
    /// </summary>
    public static class PortFinder
    {
        /// <summary>
        /// Attempts to find a random unbound port on the loopback device (localhost)
        /// within the range 1024-32768
        /// </summary>
        /// <returns></returns>
        public static int FindOpenPort()
        {
            return FindOpenPort(IPAddress.Loopback);
        }

        /// <summary>
        /// Attempts to find an unbound port on the provided ip address
        /// within the range 1024-32768
        /// </summary>
        /// <param name="forAddress"></param>
        /// <returns></returns>
        public static int FindOpenPort(IPAddress forAddress)
        {
            return FindOpenPort(forAddress, 1024, 32768);
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
                (minPort, maxPort, lastAttempt) => Next(minPort, maxPort, tried),
                attemptLogger
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
                test = portAttemptGenerator(min, max, test);
                attemptLogger?.Invoke($"Testing if port {test} is already bound on {forAddress}");
                if (PortIsInUse(forAddress, test))
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
        public static bool PortIsInUse(int port)
        {
            return PortIsInUse(IPAddress.Loopback, port);
        }

        /// <summary>
        /// Tests if the provided port is currently bound on the provided device
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsInUse(IPAddress forAddress, int port)
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
            do
            {
                result = RandomNumber.Next(min, max);
            } while (tried.Contains(result));

            return result;
        }
    }
}