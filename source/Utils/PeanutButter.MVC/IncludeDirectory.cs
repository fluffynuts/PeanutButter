namespace PeanutButter.MVC
{
    public class IncludeDirectory
    {
        public string Path { get; }
        public string SearchPattern { get; }
        public bool SearchSubdirectories { get; }
        public IncludeDirectory(string path, string searchPattern = null, bool searchSubDirectories = false)
        {
            Path = path;
            SearchPattern = searchPattern;
            SearchSubdirectories = searchSubDirectories;
        }
    }
}
