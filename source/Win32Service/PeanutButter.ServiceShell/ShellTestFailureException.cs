using System;

namespace PeanutButter.ServiceShell
{
    /// <summary>
    /// Thrown when a test run for the shell didn't go as planned
    /// </summary>
    public class ShellTestFailureException: Exception
    {
        /// <inheritdoc />
        public ShellTestFailureException(string message): base(message)
        {
        }
    }
}