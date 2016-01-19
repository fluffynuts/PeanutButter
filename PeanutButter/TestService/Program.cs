using PeanutButter.ServiceShell;

namespace TestService
{
    public class Program
    {
        static void Main(string[] args)
        {
            Shell.RunMain<TotallyNotInterestingService>(args);
        }
    }
}
