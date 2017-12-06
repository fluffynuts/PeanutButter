using System;

namespace PeanutButter.TrayIcon
{
    /// <summary>
    /// Exception thrown when code attempts to initialize a tray icon more than once
    /// </summary>
    public class TrayIconAlreadyInitializedException : Exception
    {
        /// <summary>
        /// Creates an instance of the exception
        /// </summary>
        public TrayIconAlreadyInitializedException()
            : base("This instance of the TrayIcon has already been initialized")
        {
        }
    }
}