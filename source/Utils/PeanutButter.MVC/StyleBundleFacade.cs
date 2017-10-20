using System;
using System.Collections.Generic;
using System.Web.Optimization;

namespace PeanutButter.MVC
{
    public class StyleBundleFacade : IStyleBundle
    {
        public string[] IncludedPaths => _includedPaths.ToArray();

        private readonly List<string> _includedPaths = new List<string>();
        private readonly StyleBundle _actual;

        public StyleBundleFacade(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A StyleBundle name may not be null, empty or whitespace", nameof(name));
            _actual = new StyleBundle(name);
        }

        public Bundle Include(params string[] virtualPaths)
        {
            _includedPaths.AddRange(virtualPaths);
            return _actual.Include(virtualPaths);
        }
    }
}