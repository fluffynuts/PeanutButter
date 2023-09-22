using System;

namespace PeanutButter.FileSystem
{
    public class PathNotFoundException : Exception
    {
        public PathNotFoundException(
            string path
        ) : base($"Path not found: '{path}'")
        {
        }
    }
}