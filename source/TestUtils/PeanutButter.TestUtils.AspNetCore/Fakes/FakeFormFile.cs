using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeFormFile : IFormFile
    {
        private Stream _content;

        public FakeFormFile()
        {
        }

        public FakeFormFile(
            string content,
            string name,
            string fileName
        ) : this(content, name, fileName, null)
        {
        }

        public FakeFormFile(
            string content,
            string name,
            string fileName,
            string mimeType
        )
            : this(Encoding.UTF8.GetBytes(content), name, fileName, mimeType)
        {
        }

        public FakeFormFile(byte[] bytes, string name, string fileName)
            : this(bytes, name, fileName, null)
        {
        }

        public FakeFormFile(byte[] bytes, string name, string fileName, string mimeType)
            : this(new MemoryStream(bytes), name, fileName, mimeType)
        {
        }

        public FakeFormFile(Stream content, string name, string fileName)
            : this(content, name, fileName, null)
        {
        }

        public FakeFormFile(
            Stream content,
            string name,
            string fileName,
            string mimeType
        )
        {
            _content = content;
            Name = name;
            FileName = fileName ?? name;
            ContentType = mimeType ?? MIMEType.GuessForFileName(FileName);
        }


        public Stream OpenReadStream()
        {
            // provide a new stream so that disposal
            // doesn't trash _content
            return new MemoryStream(
                _content.ReadAllBytes()
            );
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

        public void SetContent(Stream stream)
        {
            _content = stream;
        }
    }
}