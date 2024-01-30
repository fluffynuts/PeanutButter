using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Add fluency anywhere - as if all things were buildable!
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class FluencyExtensions
    {
        /// <summary>
        /// Add fluency for any object - all things can be builders now
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="mutator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T With<T>(
            this T subject,
            Action<T> mutator
        )
        {
            Validate.That(mutator, nameof(mutator)).IsNotNull();
            mutator(subject);
            return subject;
        }

        /// <summary>
        /// Apply a transform to every object in a collection
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="mutator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> WithAll<T>(
            this IList<T> subject,
            Action<T> mutator
        )
        {
            return WithAllInternal(subject, mutator);
        }

        /// <summary>
        /// Apply a transform to every object in an array
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="mutator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] WithAll<T>(
            this T[] subject,
            Action<T> mutator
        )
        {
            return WithAllInternal(
                subject,
                mutator
            );
        }

        private static TCollection WithAllInternal<TCollection, TItem>(
            TCollection subject,
            Action<TItem> mutator
        ) where TCollection : IList<TItem>
        {
            Validate.That(subject, nameof(subject)).IsNotNull()
                .And.That(mutator, nameof(mutator)).IsNotNull();
            foreach (var t in subject)
            {
                mutator(t);
            }

            return subject;
        }
    }
}