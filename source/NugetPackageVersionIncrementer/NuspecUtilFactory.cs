namespace NugetPackageVersionIncrementer
{
    public interface INuspecUtilFactory
    {
        INuspecUtil LoadNuspecAt(string path);
    }
    public class NuspecUtilFactory: INuspecUtilFactory
    {
        public INuspecUtil LoadNuspecAt(string path)
        {
            var util = new NuspecUtil();
            util.LoadNuspecAt(path);
            return util;
        }
    }
}