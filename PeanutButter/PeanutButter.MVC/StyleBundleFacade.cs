using System.Collections.Generic;
using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public class StyleBundleFacade: IStyleBundle
    {
        public string[] IncludedPaths { get; private set; }
        private List<string> _includedPaths = new List<string>();

        private StyleBundle _actual;

        public StyleBundleFacade(string name)
        {
            this._actual = new StyleBundle(name);
        }

        public Bundle Include(params string[] virtualPaths)
        {
            this._includedPaths.AddRange(virtualPaths);
            return this._actual.Include(virtualPaths);
        }
    }
}