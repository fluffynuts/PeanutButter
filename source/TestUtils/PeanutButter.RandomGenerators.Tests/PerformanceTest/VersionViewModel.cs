namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class VersionViewModel
{
    public string VersionNumber => GetType().Assembly.GetName().Version.ToString();
}