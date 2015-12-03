using System.IO;

namespace PeanutButter.TempDb
{
    public static class TempDbHints
    {
        private static object _pathLock = new object();
        public static string PreferredBasePath
        {
            get
            {
                lock (_pathLock)
                {
                    if (FolderExists(_preferredBasePath))
                        return _preferredBasePath;
                    return Path.GetTempPath();
                }
            }
            set {
                lock (_pathLock)
                {
                    _preferredBasePath = value;
                }
            }
        }

        private static bool FolderExists(string preferredBasePath)
        {
            if (Directory.Exists(preferredBasePath))
                return true;
            try
            {
                Directory.CreateDirectory(preferredBasePath);
                return Directory.Exists(preferredBasePath);
            }
            catch
            {
                return false;
            }
        }

        private static string _preferredBasePath;
        static TempDbHints()
        {
            _preferredBasePath = Path.GetTempPath();
        }
    }
}