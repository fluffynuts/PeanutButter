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

        /// <summary>
        /// PortFinder will store ports which have been handed out
        /// before so as to avoid race conditions between contenders.
        /// Ordinarily, this shouldn't be a problem - there are 64k
        /// ports to choose from - but if you are sure you don't mind
        /// a conflict and have the need to, you can reset the used
        /// history here.
        /// </summary>
        public static void ResetUsedHistory()
        {
            lock (Used)
            {
                Used.Clear();
            }
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
        /// Attempt to find an open port on the ipv4 loopback interface
        /// (localhost / 127.0.0.1), starting at the provided
        /// value and sequentially testing ports in order, until
        /// one can (probably) be used.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int FindOpenPortFrom(
            int start
        )
        {
            return FindOpenPortFrom(
                IPAddress.Loopback,
                start
            );
        }

        /// <summary>
        /// Attempt to find an open port, starting at the provided
        /// value and sequentially testing ports in order, until
        /// one can (probably) be used.
        /// </summary>
        /// <param name="forAddress"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int FindOpenPortFrom(
            IPAddress forAddress,
            int start
        )
        {
            return FindOpenPort(
                forAddress,
                start,
                MAX_PORT,
                SequentialPortLister,
                NullLogger
            );
        }

        private static int SequentialPortLister(
            int min,
            int max,
            int current
        )
        {
            if (current < min)
            {
                return min;
            }

            if (current > max)
            {
                return max;
            }
            return current + 1;
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
            var current = 0;
            for (var i = 0; i < maxTries; i++)
            {
                lock (Used)
                {
                    do
                    {
                        current = portAttemptGenerator(min, max, current);
                    } while (Used.Contains(current));

                    Used.Add(current);
                }

                attemptLogger?.Invoke($"Testing if port {current} is already bound on {forAddress}");
                if (PortIsActivelyInUse(forAddress, current))
                {
                    continue;
                }

                attemptLogger?.Invoke($"Looks like {forAddress}:{current} is not bound");
                return current;
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