using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Provides utility extensions on Stream objects
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    static class StreamExtensions
{
    /// <summary>
    /// Reads all bytes from a stream
    /// </summary>
    /// <param name="src">Source stream to read from</param>
    /// <returns>Byte array of the data read from the stream</returns>
    public static byte[] ReadAllBytes(this Stream src)
    {
        return src is null
            ? null
            : ReadAllBytesFrom(src);
    }

    /// <summary>
    /// Reads all bytes from a stream
    /// </summary>
    /// <param name="src">Source stream to read from</param>
    /// <returns>Byte array of the data read from the stream</returns>
    public static async Task<byte[]> ReadAllBytesAsync(this Stream src)
    {
        return src is null
            ? null
            : await ReadAllBytesAsyncFrom(src);
    }

    /// <summary>
    /// Writes all given bytes to a stream
    /// </summary>
    /// <param name="source">Target stream to write to</param>
    /// <param name="data">Binary data to write</param>
    /// <exception cref="IOException">Thrown when the target stream is null</exception>
    public static void WriteAllBytes(this Stream source, byte[] data)
    {
        if (source is null)
        {
            throw new IOException("Source stream is null");
        }

        if (data is null || data.Length == 0)
        {
            return;
        }

        source.Seek(0, SeekOrigin.Begin);
        source.SetLength(0);
        source.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Writes all given bytes to a stream
    /// </summary>
    /// <param name="source">Target stream to write to</param>
    /// <param name="data">Binary data to write</param>
    /// <exception cref="IOException">Thrown when the target stream is null</exception>
    public static async Task WriteAllBytesAsync(this Stream source, byte[] data)
    {
        if (source is null)
        {
            throw new IOException("Source stream is null");
        }

        if (data is null || data.Length == 0)
        {
            return;
        }

        source.Seek(0, SeekOrigin.Begin);
        source.SetLength(0);
        await source.WriteAsync(data, 0, data.Length);
    }

    /// <summary>
    /// Rewinds the current stream pointer to the beginning of the stream (when supported by the stream)
    /// </summary>
    /// <param name="src">Source stream to rewind</param>
    public static void Rewind(this Stream src)
    {
        if (src is null)
        {
            throw new ArgumentException("cannot rewind null stream", nameof(src));
        }

        if (src.Position == 0)
        {
            return;
        }

        if (!src.CanSeek)
        {
            throw new ArgumentException($"Cannot seek in stream of type: {src.GetType()}");
        }

        src.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Appends binary data to the end of the stream
    /// </summary>
    /// <param name="target">Target stream to write to</param>
    /// <param name="data">Binary data to write</param>
    /// <exception cref="IOException">Thrown when the target stream is null</exception>
    public static void Append(this Stream target, byte[] data)
    {
        if (target is null)
        {
            throw new IOException("Target stream is null");
        }

        target.Seek(0, SeekOrigin.End);
        target.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Appends binary data to the end of the stream
    /// </summary>
    /// <param name="target">Target stream to write to</param>
    /// <param name="data">Binary data to write</param>
    /// <exception cref="IOException">Thrown when the target stream is null</exception>
    public static async Task AppendAsync(this Stream target, byte[] data)
    {
        if (target is null)
        {
            throw new IOException("Target stream is null");
        }

        target.Seek(0, SeekOrigin.End);
        await target.WriteAsync(data, 0, data.Length);
    }

    /// <summary>
    /// Attempts to get a string representation of the contents of a stream
    /// </summary>
    /// <param name="src">Source stream to read</param>
    /// <param name="encoding">Optional encoding to use (defaults to UTF8 when null)</param>
    /// <returns>A string representation of the stream</returns>
    public static string AsString(this Stream src, Encoding encoding = null)
    {
        var buffer = src.ReadAllBytes();
        encoding ??= Encoding.UTF8;
        var asString = encoding.GetString(buffer);
        var firstNull = asString.IndexOf('\0');
        if (firstNull == -1)
        {
            firstNull = asString.Length;
        }

        return asString.Substring(0, firstNull);
    }

    /// <summary>
    /// Attempts to get a string representation of the contents of a stream
    /// </summary>
    /// <param name="src">Source stream to read</param>
    /// <param name="encoding">Optional encoding to use (defaults to UTF8 when null)</param>
    /// <returns>A string representation of the stream</returns>
    public static async Task<string> AsStringAsync(this Stream src, Encoding encoding = null)
    {
        var buffer = await src.ReadAllBytesAsync();
        encoding ??= Encoding.UTF8;
        var asString = encoding.GetString(buffer);
        var firstNull = asString.IndexOf('\0');
        if (firstNull == -1)
        {
            firstNull = asString.Length;
        }

        return asString.Substring(0, firstNull);
    }

    /// <summary>
    /// Invokes WriteString() with the UTF8 Encoding
    /// </summary>
    /// <param name="stream">Stream to operate on</param>
    /// <param name="data">String data to write</param>
    public static void WriteString(this Stream stream, string data)
    {
        stream.WriteString(data, Encoding.UTF8);
    }

    /// <summary>
    /// Invokes WriteString() with the UTF8 Encoding
    /// </summary>
    /// <param name="stream">Stream to operate on</param>
    /// <param name="data">String data to write</param>
    public static async Task WriteStringAsync(this Stream stream, string data)
    {
        await stream.WriteStringAsync(data, Encoding.UTF8);
    }

    /// <summary>
    /// Writes a string to a stream with the provided encoding
    /// </summary>
    /// <param name="stream">Stream to write to</param>
    /// <param name="data">String data to write</param>
    /// <param name="encoding">Encoding to use</param>
    public static void WriteString(this Stream stream, string data, Encoding encoding)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        var bytes = encoding.GetBytes(data);
        stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Writes a string to a stream with the provided encoding
    /// </summary>
    /// <param name="stream">Stream to write to</param>
    /// <param name="data">String data to write</param>
    /// <param name="encoding">Encoding to use</param>
    public static async Task WriteStringAsync(this Stream stream, string data, Encoding encoding)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        var bytes = encoding.GetBytes(data);
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Saves a stream to a local file
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filePath"></param>
    public static void Save(
        this Stream stream,
        string filePath
    )
    {
        if (stream.CanSeek)
        {
            stream.Rewind();
        }

        EnsureFolderExistsFor(filePath);

        using var fileStream = File.Create(filePath);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// Saves a stream to a local file
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filePath"></param>
    public static async Task SaveAsync(
        this Stream stream,
        string filePath
    )
    {
        if (stream.CanSeek)
        {
            stream.Rewind();
        }

        EnsureFolderExistsFor(filePath);

        using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);
    }

    private static void EnsureFolderExistsFor(string filePath)
    {
        var containingFolder = Path.GetDirectoryName(filePath);
        if (!(containingFolder is null) &&
            !Directory.Exists(containingFolder))
        {
            Directory.CreateDirectory(containingFolder);
        }
    }

    private static byte[] ReadAllBytesFrom(Stream src)
    {
        if (src is MemoryStream memStream)
        {
            return memStream.ToArray();
        }

        if (src.CanSeek)
        {
            src.Rewind();
        }

        using var stream = new MemoryStream();
        src.CopyTo(stream);

        if (src.CanSeek)
        {
            src.Rewind();
        }

        return stream.ToArray();
    }

    private static async Task<byte[]> ReadAllBytesAsyncFrom(Stream src)
    {
        if (src is MemoryStream memStream)
        {
            return memStream.ToArray();
        }

        if (src.CanSeek)
        {
            src.Rewind();
        }

        using var stream = new MemoryStream();
        await src.CopyToAsync(stream);

        if (src.CanSeek)
        {
            src.Rewind();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Appends a string to a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    public static void AppendString(
        this Stream stream,
        string value
    )
    {
        stream.Append(
            Encoding.UTF8.GetBytes(value)
        );
    }

    /// <summary>
    /// Appends a text line to a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="line"></param>
    public static void AppendLine(
        this Stream stream,
        string line
    )
    {
        stream.AppendString($"{line}\n");
    }

    private static void NoOp(Eol obj)
    {
    }

    /// <summary>
    /// Reads the stream, line-for-line, with the UTF8 encoding.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this Stream stream
    )
    {
        return stream.ReadLines(NoOp);
    }

    /// <summary>
    /// Reads the stream, line-for-line, with the UTF8 encoding.
    /// When a newline is encountered, the onNewLine func is called
    /// with the kind of line ending last encountered.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="onNewline"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this Stream stream,
        Action<Eol> onNewline
    )
    {
        return stream.ReadLines(Encoding.UTF8, onNewline);
    }

    /// <summary>
    /// Reads the stream, line-for-line, with the provided encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this Stream stream,
        Encoding encoding
    )
    {
        return stream.ReadLines(encoding, NoOp);
    }

    /// <summary>
    /// Reads the stream, line-for-line, with the provided encoding.
    /// When a newline is encountered, the onNewLine func is called
    /// with the kind of line ending last encountered.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="onNewLine"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(
        this Stream stream,
        Encoding encoding,
        Action<Eol> onNewLine
    )
    {
        var bytes = new List<byte>();
        int c;
        var collected = 0;
        var danglingCarriageReturn = false;
        while ((c = stream.ReadByte()) != -1)
        {
            if (c == '\r')
            {
                danglingCarriageReturn = true;
                continue;
            }

            if (c == '\n')
            {
                yield return encoding.GetString(
                    bytes.ToArray()
                );
                collected = 0;
                bytes.Clear();
                onNewLine?.Invoke(
                    danglingCarriageReturn
                        ? Eol.CrLf
                        : Eol.Lf
                );
                danglingCarriageReturn = false;
                continue;
            }

            if (danglingCarriageReturn)
            {
                bytes.Add((byte)'\r');
                danglingCarriageReturn = false;
            }

            collected++;
            bytes.Add((byte)c);
        }

        if (collected > 0)
        {
            yield return encoding.GetString(
                bytes.ToArray()
            );
        }
    }

    /// <summary>
    /// Read one line from the stream, encoded as utf8
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ReadLine(
        this Stream stream
    )
    {
        return stream.ReadLine(Encoding.UTF8);
    }

    /// <summary>
    /// Read one line from the stream, with the provided encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadLine(
        this Stream stream,
        Encoding encoding
    )
    {
        var bytes = new List<byte>();
        int c;
        var collected = 0;
        var danglingCarriageReturn = false;
        while ((c = stream.ReadByte()) != -1)
        {
            if (c == '\r')
            {
                danglingCarriageReturn = true;
                continue;
            }

            if (c == '\n')
            {
                return encoding.GetString(
                    bytes.ToArray()
                );
            }

            if (danglingCarriageReturn)
            {
                bytes.Add((byte)'\r');
                danglingCarriageReturn = false;
            }

            collected++;
            bytes.Add((byte)c);
        }

        if (collected > 0)
        {
            return encoding.GetString(
                bytes.ToArray()
            );
        }

        return null;
    }

    /// <summary>
    /// Read all content of a stream as text
    /// with utf8 encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ReadAllText(
        this Stream stream
    )
    {
        return stream.ReadAllText(Encoding.UTF8);
    }

    /// <summary>
    /// Read all content of a stream as text
    /// with the provided encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadAllText(
        this Stream stream,
        Encoding encoding
    )
    {
        return encoding.GetString(
            stream.ReadAllBytes()
        );
    }

    /// <summary>
    /// Read all content of a stream as text
    /// with utf8 encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> ReadAllTextAsync(
        this Stream stream
    )
    {
        return await stream.ReadAllTextAsync(Encoding.UTF8);
    }

    /// <summary>
    /// Read all content of a stream as text
    /// with the provided encoding
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<string> ReadAllTextAsync(
        this Stream stream,
        Encoding encoding
    )
    {
        return encoding.GetString(
            await stream.ReadAllBytesAsync()
        );
    }
}

/// <summary>
/// Specifies the expected eol character for a text blob
/// </summary>
public enum Eol
{
    /// <summary>
    /// Line-feed only (eg Linux)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    Lf,

    /// <summary>
    /// Carriage-return, Line-feed (eg Windows)
    /// </summary>
    CrLf
}