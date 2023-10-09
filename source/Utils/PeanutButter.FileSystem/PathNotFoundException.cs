using System;

namespace PeanutButter.FileSystem
{
    /// <summary>
    /// Thrown when a provided path is not found
    /// </summary>
    public class PathNotFoundException : Exception
    {
        /// <summary>
        /// Thrown when a provided path is not found
        /// </summary>
        /// <param name="path"></param>
        public PathNotFoundException(
            string path
        ) : base($"Path not found: '{path}'")
        {
        }
    }
}