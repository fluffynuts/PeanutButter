using System;
// ReSharper disable InheritdocConsiderUsage

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Exception thrown when a property cannot be found by name
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public 
#endif
        class MemberNotFoundException : Exception
    {
        /// <summary>
        /// Constructs a new MemberNotFoundException
        /// </summary>
        /// <param name="type">The Type being searched for the property</param>
        /// <param name="propertyName">The name of the property which was not found</param>
        public MemberNotFoundException(Type type, string propertyName):
            base("Property '" + propertyName + "' not found on type '" + type.Name + "'")
        {
        }
    }
}