using System.Collections.Generic;
using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public class ScriptBundleFacade : IScriptBundle
    {
        public string Name { get; private set; }
        public string[] IncludedPaths { get { return _includedPaths.ToArray(); }}
        public IncludeDirectory[] IncludedDirectories { get { return _includedDirectories.ToArray(); }}

        private ScriptBundle _actual;
        private List<IncludeDirectory> _includedDirectories = new List<IncludeDirectory>();
        private List<string> _includedPaths = new List<string>();
        public ScriptBundleFacade(string name)
        {
            this._actual = new ScriptBundle(name);
        }

        public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        {
            this._includedDirectories.Add(new IncludeDirectory(directoryVirtualPath, searchPattern));
            return this._actual.IncludeDirectory(directoryVirtualPath, searchPattern);
        }

        public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories)
        {
            this._includedDirectories.Add(new IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories));
            return this._actual.IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories);
        }

        public Bundle Include(params string[] relativePaths)
        {
            this._includedPaths.AddRange(relativePaths);
            return this._actual.Include(relativePaths);
        }
    }
}