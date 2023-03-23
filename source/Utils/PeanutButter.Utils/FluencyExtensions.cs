using System;

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
            mutator(subject);
            return subject;
        }
    }
}