using System;
using System.Collections.Generic;
using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public class ScriptBundleFacade : IScriptBundle
    {
        public string Name { get; }
        public string[] IncludedPaths => _includedPaths.ToArray();
        public IncludeDirectory[] IncludedDirectories => _includedDirectories.ToArray();

        private readonly ScriptBundle _actual;
        private readonly List<IncludeDirectory> _includedDirectories = new List<IncludeDirectory>();
        private readonly List<string> _includedPaths = new List<string>();

        public ScriptBundleFacade(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "A ScriptBundle name may not be null, empty or whitespace",
                    nameof(name)
                );
            }

            Name = name;
            _actual = new ScriptBundle(name);
        }

        public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        {
            _includedDirectories.Add(new IncludeDirectory(directoryVirtualPath, searchPattern));
            return _actual.IncludeDirectory(directoryVirtualPath, searchPattern);
        }

        public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories)
        {
            _includedDirectories.Add(new IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories));
            return _actual.IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories);
        }

        public Bundle Include(params string[] relativePaths)
        {
            _includedPaths.AddRange(relativePaths);
            return _actual.Include(relativePaths);
        }
    }
}