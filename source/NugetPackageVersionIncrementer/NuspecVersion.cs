using System.Linq;

namespace NugetPackageVersionIncrementer
{
    public class NuspecVersion
    {
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public NuspecVersion(string version)
        {
            if (!version.StartsWith("["))
            {
                Minimum = version;
                Maximum = version;
            }
            var parts = version.Trim('[', ']').Split(',');
            Minimum = parts.First();
            Maximum = parts.Skip(1).FirstOrDefault() ?? Minimum;
        }

        public override string ToString()
        {
            if (Minimum == Maximum)
            {
                return Minimum;
            }

            return $"[{Minimum},{Maximum}]";
        }

    }
}