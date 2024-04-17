using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConstantNullCoalescingCondition

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
using PeanutButter.Utils;
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Implements a fake form file
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeFormFile : IFormFile, IFake
{
    private Stream _content;
    private const string CONTENT_TYPE_TEXT = "text/plain";
    private const string CONTENT_TYPE_BINARY = "application/octet-stream";

    /// <summary>
    /// Default constructor: create an empty form file
    /// </summary>
    public FakeFormFile() : this(Array.Empty<byte>(), "", "")
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    public FakeFormFile(
        string content,
        string name,
        string fileName
    ) : this(content, name, fileName, CONTENT_TYPE_TEXT)
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <param name="mimeType"></param>
    public FakeFormFile(
        string content,
        string name,
        string fileName,
        string mimeType
    )
        : this(Encoding.UTF8.GetBytes(content), name, fileName, mimeType)
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    public FakeFormFile(byte[] content, string name, string fileName)
        : this(content, name, fileName, CONTENT_TYPE_BINARY)
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <param name="mimeType"></param>
    public FakeFormFile(byte[] content, string name, string fileName, string mimeType)
        : this(new MemoryStream(content), name, fileName, mimeType)
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    public FakeFormFile(Stream content, string name, string fileName)
        : this(content, name, fileName, null)
    {
    }

    /// <summary>
    /// Create a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <param name="mimeType"></param>
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
        ContentType = mimeType ?? MimeType.GuessForFileName(FileName);
        ContentDisposition = $"attachment; filename=${FileName}";
    }


    /// <inheritdoc />
    public Stream OpenReadStream()
    {
        // provide a new stream so that disposal
        // doesn't trash _content
        return new MemoryStream(
            _content.ReadAllBytes()
        );
    }

    /// <summary>
    /// convenience property to read the content of the file
    /// </summary>
    /// <returns></returns>
    public byte[] Content
    {
        get
        {
            using var memStream = new MemoryStream();
            _content.Rewind();
            _content.CopyTo(memStream);
            return memStream.ToArray();
        }
    }

    /// <inheritdoc />
    public void CopyTo(Stream target)
    {
        _content.Position = 0;
        _content.CopyTo(target);
    }

    /// <inheritdoc />
    public Task CopyToAsync(
        Stream target,
        CancellationToken cancellationToken = new()
    )
    {
        return _content.CopyToAsync(target, cancellationToken);
    }

    /// <inheritdoc />
    public string ContentType { get; set; }

    /// <inheritdoc />
    public string ContentDisposition { get; set; }

    /// <inheritdoc />
    public IHeaderDictionary Headers
    {
        get => _headers ??= new FakeHeaderDictionary();
        set => _headers = value ?? new FakeHeaderDictionary();
    }

    private IHeaderDictionary _headers;

    /// <inheritdoc />
    public long Length => _content.Length;

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string FileName { get; set; }

    /// <summary>
    /// Sets the content for the file (overwrites)
    /// </summary>
    /// <param name="stream"></param>
    public void SetContent(Stream stream)
    {
        _content = new MemoryStream();
        stream.CopyTo(_content);
    }

    /// <summary>
    /// Sets the content for the file (overwrites)
    /// </summary>
    /// <param name="content"></param>
    public void SetContent(string content)
    {
        SetContent(content, Encoding.UTF8);
    }

    /// <summary>
    /// Sets the content for the file (overwrites)
    /// </summary>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    public void SetContent(string content, Encoding encoding)
    {
        _content = new MemoryStream(encoding.GetBytes(content));
    }

    /// <summary>
    /// Sets the content for the file (overwrites)
    /// </summary>
    /// <param name="content"></param>
    public void SetContent(byte[] content)
    {
        _content = new MemoryStream(content);
    }

    /// <summary>
    /// Replaces the content stream, perhaps useful for a lazy
    /// evaluation.
    /// </summary>
    /// <param name="stream"></param>
    public void SetContentStream(Stream stream)
    {
        _content = stream;
    }
}