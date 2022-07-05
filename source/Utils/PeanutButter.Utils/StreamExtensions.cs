using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides utility extensions on Stream objects
    /// </summary>
    public static class StreamExtensions
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
                throw new ArgumentException(nameof(src));
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
            if (src.CanSeek)
            {
                src.Rewind();
                var buffer = new byte[src.Length];
                src.Read(buffer, 0, buffer.Length);
                return buffer;
            }

            using var stream = new MemoryStream();
            var readCount = 0;
            do
            {
                var thisPart = new byte[DEFAULT_CAPACITY];
                readCount = src.Read(thisPart, 0, DEFAULT_CAPACITY);
                stream.Write(thisPart, 0, readCount);
            } while (readCount > 0);

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
                var buffer = new byte[src.Length];
                await src.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }

            using var stream = new MemoryStream();
            var readCount = 0;
            do
            {
                var thisPart = new byte[DEFAULT_CAPACITY];
                readCount = await src.ReadAsync(thisPart, 0, DEFAULT_CAPACITY);
                stream.Write(thisPart, 0, readCount);
            } while (readCount > 0);

            return stream.ToArray();
        }

        private const int DEFAULT_CAPACITY = 1024 * 1024 * 1024; // 1mb default
    }
}