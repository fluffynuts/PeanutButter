using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public interface IStyleBundle
    {
        string[] IncludedPaths { get; }
        Bundle Include (params string[] virtualPaths);
    }
}