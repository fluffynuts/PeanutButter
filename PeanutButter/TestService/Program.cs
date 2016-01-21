using PeanutButter.ServiceShell;

namespace TestService
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static int Main(string[] args)
        {
            return Shell.RunMain<TotallyNotInterestingService>(args);
        }
    }
}
