using System;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.Runner
{
    public class ShowSupportedEngines : Exception
    {
        public ShowSupportedEngines(string[] availableEngines)
            : base($"Supported engine{(availableEngines.Length == 1 ? "" : "s")}: ${availableEngines.JoinWith(", ")}")
        {
        }
    }
}