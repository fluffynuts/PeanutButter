using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeFormCollection : StringValueMap, IFormCollection
    {
        public FakeFormCollection(
            IDictionary<string, StringValues> store
        ): base(store)
        {
        }

        public FakeFormCollection() 
            : base(StringComparer.Ordinal)
        {
        }

        public IDictionary<string, StringValues> FormValues
        {
            get => Store;
            set => Store = value;
        }

        public IFormFileCollection Files
        {
            get => _files ??= new FakeFormFileCollection();
            set => _files = value ?? new FakeFormFileCollection();
        }

        private IFormFileCollection _files = new FakeFormFileCollection();

        public void AddFile(IFormFile formFile)
        {
            var asFake = _files as FakeFormFileCollection
                ?? throw new InvalidImplementationException(_files, "FormFileCollection cannot be modified");
            asFake.Add(formFile);
        }
    }
}