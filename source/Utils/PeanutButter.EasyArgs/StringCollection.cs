using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
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