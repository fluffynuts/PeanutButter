using System;

namespace PeanutButter.TempDb.MySql.Base
{
    internal class UnableToInitializeMySqlException
        : Exception
    {
        public UnableToInitializeMySqlException(
            string errorOutput
        ) : base($"Unable to initialize mysql: {errorOutput}")
        {
        }
    }
}