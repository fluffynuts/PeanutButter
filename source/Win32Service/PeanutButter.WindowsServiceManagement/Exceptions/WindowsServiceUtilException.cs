using System;

namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    public class WindowsServiceUtilException : Exception
    {
        public WindowsServiceUtilException(string message)
            : base (message)
        {
        }
    }
}