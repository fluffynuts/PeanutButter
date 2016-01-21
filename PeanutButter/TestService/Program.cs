using PeanutButter.ServiceShell;

namespace TestService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Shell.RunMain<TotallyNotInterestingService>(args);
        }
    }
}
