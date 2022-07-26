using System;

namespace PeanutButter.TempDb.MySql.Base
{
    public class UnableToInitializeMySqlException
        : Exception
    {
        public UnableToInitializeMySqlException(
            string errorOutput
        ) : base($"Unable to initialize mysql: {errorOutput}")
        {
        }
    }
}