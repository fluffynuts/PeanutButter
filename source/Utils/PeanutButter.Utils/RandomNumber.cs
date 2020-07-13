using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// provides a singleton wrapper around Random.Next
    ///    to reduce the chances of clashing
    /// </summary>
    public static class RandomNumber
    {
        private static readonly Random RandomSource 
            = new Random(Guid.NewGuid().GetHashCode());
        
        /// <summary>
        /// Wraps Random.Next
        /// </summary>
        /// <returns></returns>
        public static int Next()
        {
            return RandomSource.Next();
        }

        /// <summary>
        /// Wraps Random.Next
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int maxValue)
        {
            return RandomSource.Next(maxValue);
        }
        
        /// <summary>
        /// Wraps Random.Next
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int minValue, int maxValue)
        {
            return RandomSource.Next(minValue, maxValue);
        }

        /// <summary>
        /// Wraps Random.NextDouble
        /// </summary>
        /// <returns></returns>
        public static double NextDouble()
        {
            return RandomSource.NextDouble();
        }

        /// <summary>
        /// Wraps Random.NextBytes
        /// </summary>
        /// <param name="buffer"></param>
        public static void NextBytes(byte[] buffer)
        {
            RandomSource.NextBytes(buffer);
        }
    }

}