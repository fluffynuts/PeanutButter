using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.MVC
{
    public class IncludeDirectory
    {
        public string Path { get; private set; }
        public string SearchPattern { get; private set; }
        public bool SearchSubdirectories { get; private set; }
        public IncludeDirectory(string path, string searchPattern = null, bool searchSubDirectories = false)
        {
            this.Path = path;
            this.SearchPattern = searchPattern;
            this.SearchSubdirectories = searchSubDirectories;
        }
    }
}
