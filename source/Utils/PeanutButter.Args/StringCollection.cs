using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Args
{
    internal class StringCollection : IHasValue
    {
        public string SingleValue => _values.FirstOrDefault();
        public string[] AllValues => _values.ToArray();

        private readonly List<string> _values = new();

        public void Add(string value)
        {
            _values.Add(value);
        }
    }
}