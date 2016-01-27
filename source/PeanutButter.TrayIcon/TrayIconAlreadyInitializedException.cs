using System;

namespace PeanutButter.TrayIcon
{
    public class TrayIconAlreadyInitializedException : Exception
    {
        public TrayIconAlreadyInitializedException() : base("This instance of the TrayIcon has already been initialized")
        {
        }
    }
}