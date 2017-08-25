using System.Linq;

namespace PeanutButter.Utils.Tests
{
    public static class DeepEqualityTesterExtensions
    {
        public static string PrintErrors(this DeepEqualityTester tester)
        {
            return tester.Errors.Any()
                ? "* " + tester.Errors.JoinWith("\n *")
                : "";
        }
    }
}