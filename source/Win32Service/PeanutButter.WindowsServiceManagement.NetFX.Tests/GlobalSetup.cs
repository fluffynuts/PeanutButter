using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.WindowsServiceManagement.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        private string _myFolder;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("Windows-specific testing");
                return;
            }

            var testServiceBase = FindTestServiceDir();
            var config =
#if DEBUG
                "Debug";
#else
                "Release";
#endif
            var fullPath = Path.Combine(testServiceBase, "bin", config);
            if (!Directory.Exists(fullPath))
            {
                throw new Exception(
                    $"Can't find {fullPath}"
                );
            }

            Directory.EnumerateFiles(fullPath).ForEach(f =>
            {
                var target = Path.Combine(MyFolder, f);
                var src = Path.Combine(fullPath, f);
                try
                {
                    File.Copy(src, target);
                }
                catch
                {
                    /* ignore: may be a dep of this test asm too */
                }
            });
        }
        
        private string MyFolder
            => _myFolder ??= FindMyFolder();

        private string FindMyFolder()
        {
            return Path.GetDirectoryName(
                new Uri(typeof(GlobalSetup)
                    .Assembly.Location
                ).LocalPath
            );
        }

        private string FindTestServiceDir()
        {
            var search = MyFolder;
            while (search != null)
            {
                var foldersHere = Directory.EnumerateDirectories(search);
                var testServiceDir = foldersHere.FirstOrDefault(
                    f => Path.GetFileName(f.ToLower()) == "testservice"
                );
                if (testServiceDir != null)
                {
                    return testServiceDir;
                }

                search = Path.GetDirectoryName(search);
            }

            throw new Exception("Can't find test service folder");
        }
    }
}