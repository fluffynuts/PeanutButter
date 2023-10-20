using System;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Base
{
    public class ProcessStartFailureException : Exception
    {
        public string Executable { get; }
        public string[] Arguments { get; }

        public ProcessStartFailureException(
            string executable,
            string[] args
        ) : base($"Unable to start process: {executable} {args.Select(ProcessIO.QuoteIfNecessary).JoinWith(" ")}")
        {
            Executable = executable;
            Arguments = args;
        }
    }
}