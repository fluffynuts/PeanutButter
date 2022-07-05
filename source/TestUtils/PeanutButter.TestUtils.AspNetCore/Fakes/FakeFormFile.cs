using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeFormFile : IFormFile
    {
        private readonly MemoryStream _content;

        public FakeFormFile(string content, string name, string fileName)
            : this(Encoding.UTF8.GetBytes(content), name, fileName)
        {
        }

        private FakeFormFile(byte[] bytes, string name, string fileName)
            : this(new MemoryStream(bytes), name, fileName)
        {
        }

        private FakeFormFile(MemoryStream content, string name, string fileName)
        {
            _content = content;
            Name = name;
            FileName = name ?? fileName;
        }


        public Stream OpenReadStream()
        {
            _content.Position = 0;
            return new MemoryStream(_content.ToArray());
        }

        public void CopyTo(Stream target)
        {
            _content.Position = 0;
            _content.CopyTo(target);
        }

        public Task CopyToAsync(
            Stream target,
            CancellationToken cancellationToken = new()
        )
        {
            return _content.CopyToAsync(target);
        }

        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }

        public IHeaderDictionary Headers
        {
            get => _headers ??= new FakeHeaderDictionary();
            set => _headers = value ?? new FakeHeaderDictionary();
        }

        private IHeaderDictionary _headers;

        public long Length => _content.Length;
        public string Name { get; set; }
        public string FileName { get; set; }
    }
}