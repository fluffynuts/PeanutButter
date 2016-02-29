using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public interface IScriptBundle
    {
        string Name { get; }
        string[] IncludedPaths { get; }
        IncludeDirectory[] IncludedDirectories { get; }
        Bundle Include(params string[] relativePaths);
        Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern);
        Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories);
    }
}