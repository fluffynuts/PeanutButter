namespace RandomBuilderPerformanceTest.Fortel
{
    public class VersionViewModel
    {
        public string VersionNumber
        {
            get { return this.GetType().Assembly.GetName().Version.ToString(); }
        }
    }
}