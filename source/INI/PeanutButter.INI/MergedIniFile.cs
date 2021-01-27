namespace PeanutButter.INI
{
    internal class MergedIniFile
    {
        public IINIFile IniFile { get; }
        public MergeStrategies MergeStrategy { get; }

        internal MergedIniFile(
            IINIFile iniFile,
            MergeStrategies mergeStrategy)
        {
            IniFile = iniFile;
            MergeStrategy = mergeStrategy;
        }
    }
}