using System;
using System.Diagnostics;

namespace PeanutButter.TempDb.MySql.Base
{
    public class ProcessStartFailureException : Exception
    {
        public ProcessStartInfo StartInfo { get; }

        public ProcessStartFailureException(
            ProcessStartInfo startInfo
        ) : base($"Unable to start process: ${startInfo.FileName} ${startInfo.Arguments}")
        {
            StartInfo = startInfo;
        }
    }
}