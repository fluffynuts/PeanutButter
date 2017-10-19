namespace NugetPackageVersionIncrementer
{
    public class LegacyPackageVersionIncrementer
    {
        public string BeforeVersion { get; private set; }
        public string AfterVersion { get; private set; }
        public void IncrementVersionOn(string path)
        {
            var readerWriter = new NuspecReaderWriter(path);
            var incrementer = new NuspecVersionIncrementer(readerWriter.NuspecXml);
            BeforeVersion = incrementer.Version;
            incrementer.IncrementMinorVersion();
            AfterVersion = incrementer.Version;
            readerWriter.NuspecXml = incrementer.GetUpdatedNuspec();
            readerWriter.Rewrite();
        }
    }
}