using Microsoft.Extensions.Primitives;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Describes a type which has
    /// </summary>
    public interface ICanBeIndexedBy<in T>
    {
        /// <summary>
        /// Indexes into the store
        /// </summary>
        /// <param name="key"></param>
        StringValues this[T key] { get; set; }
    }
}