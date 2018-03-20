using System;
using System.Diagnostics;
using System.IO;

namespace PeanutButter.TempDb
{
    public static class TempDbHints
    {
        private static readonly object PathLock = new object();

        public static string DefaultBasePath => Path.GetTempPath();

        public static string PreferredBasePath
        {
            get
            {
                lock (PathLock)
                {
                    return FolderExists(_preferredBasePath)
                        ? _preferredBasePath
                        : Path.GetTempPath();
                }
            }
            set
            {
                lock (PathLock)
                {
                    _preferredBasePath = value;
                }
            }
        }

        public static bool UsingOverrideBasePath => 
            PreferredBasePath != Path.GetTempPath();

        private static bool FolderExists(string preferredBasePath)
        {
            if (Directory.Exists(preferredBasePath))
                return true;
            try
            {
                Directory.CreateDirectory(preferredBasePath);
                return Directory.Exists(preferredBasePath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to establish folder specified by TEMPDB_BASE_PATH environment variable ({preferredBasePath})\n{ex.Message}");
                return false;
            }
        }

        private static string _preferredBasePath;

        static TempDbHints()
        {
            var env = Environment.GetEnvironmentVariable("TEMPDB_BASE_PATH");
            _preferredBasePath = string.IsNullOrWhiteSpace(env)
                ? Path.GetTempPath()
                : env;
        }
    }
}