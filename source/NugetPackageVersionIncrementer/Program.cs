using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageVersionIncrementer
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                var finder = new NuspecFinder();
                finder.FindNuspecsUnder(arg);
                foreach (var path in finder.NuspecPaths)
                {
                    try
                    {
                        var readerWriter = new NuspecReaderWriter(path);
                        var incrementer = new NuspecVersionIncrementer(readerWriter.NuspecXML);
                        var beforeVersion = incrementer.Version;
                        incrementer.IncrementMinorVersion();
                        var afterVersion = incrementer.Version;
                        readerWriter.NuspecXML = incrementer.GetUpdatedNuspec();
                        readerWriter.Rewrite();
                        Console.WriteLine("{0} : {1} => {2}", incrementer.PackageID, beforeVersion, afterVersion);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to increment version on '" + path + "'\n" + ex.Message);
                    }
                }
            }
        }
    }
}
