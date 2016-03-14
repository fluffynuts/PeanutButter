using System;

namespace PeanutButter.ServiceShell
{
    public class ShellTestFailureException: Exception
    {
        public ShellTestFailureException(string message): base(message)
        {
        }
    }
}