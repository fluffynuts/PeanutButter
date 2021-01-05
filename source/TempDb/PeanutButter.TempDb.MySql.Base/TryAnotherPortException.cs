using System;

namespace PeanutButter.TempDb.MySql.Base
{
    public class TryAnotherPortException : Exception
    {
        public TryAnotherPortException(string message) : base(message)
        {
        }
    }
}