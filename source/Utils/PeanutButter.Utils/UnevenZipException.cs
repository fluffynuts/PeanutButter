using System;
using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Thrown when an attempt is made to zip two collections of
    /// uneven size
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class UnevenZipException : Exception
    {
        /// <inheritdoc />
        public UnevenZipException() : base("Could not zip uneven collections")
        {
        }
    }

    /// <summary>
    /// Thrown when an attempt is made to zip two collections of
    /// uneven size. Also includes references to the two collections.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class UnevenZipException<T1, T2> : UnevenZipException
    {
        /// <summary>
        /// The left collection
        /// </summary>
        public IEnumerable<T1> Left { get; }

        /// <summary>
        /// The right collection
        /// </summary>
        public IEnumerable<T2> Right { get; }

        /// <inheritdoc />
        public UnevenZipException(
            IEnumerable<T1> left,
            IEnumerable<T2> right
        )
        {
            Left = left;
            Right = right;
        }
    }
}