using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeFormFileCollection : IFormFileCollection
    {
        private readonly List<IFormFile> _store = new();

        public IEnumerator<IFormFile> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _store.Count;

        public IFormFile this[int index] => _store[index];

        public IFormFile GetFile(string name)
        {
            return _store.FirstOrDefault(o => o.Name == name);
        }

        public IReadOnlyList<IFormFile> GetFiles(string name)
        {
            return _store
                .Where(o => o.Name == name)
                .ToList();
        }

        public IFormFile this[string name] => GetFile(name);

        public void Add(IFormFile formFile)
        {
            _store.Add(formFile);
        }
    }
}