using System;

namespace PeanutButter.Win32ServiceControl.Exceptions
{
    public class WindowsServiceUtilException : Exception
    {
        public WindowsServiceUtilException(string message)
            : base (message)
        {
        }
    }
}