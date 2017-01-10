using System;
using System.Reflection;

namespace PeanutButter.TestUtils.Entity
{
    public class SharedDatabaseAlreadyRegisteredException : Exception
    {
        public SharedDatabaseAlreadyRegisteredException(string name)
            : base($"The shared database {name} is already registered")
        {
        }
    }
}