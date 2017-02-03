using System.IO;
using System.Text;

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
            return src == null
                    ? null
                    : ReadAllBytesFrom(src);
        }

        /// <summary>
        /// Writes all given bytes to a stream
        /// </summary>
        /// <param name="target">Target stream to write to</param>
        /// <param name="data">Binary data to write</param>
        /// <exception cref="IOException">Thrown when the target stream is null</exception>
        public static void WriteAllBytes(this Stream target, byte[] data)
        {
            if (target == null)
                throw new IOException("Source stream is null");
            if ((data ?? new byte[] { }).Length == 0)
                return;
            target.Seek(0, SeekOrigin.Begin);
            target.SetLength(0);
            target.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Rewinds the current stream pointer to the beginning of the stream (when supported by the stream)
        /// </summary>
        /// <param name="src">Source stream to rewind</param>
        public static void Rewind(this Stream src)
        {
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
            if (target == null)
                throw new IOException("Target stream is null");
            target.Seek(0, SeekOrigin.End);
            target.Write(data, 0, data.Length);
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
            encoding = encoding ?? Encoding.UTF8;
            var asString = encoding.GetString(buffer);
            var firstNull = asString.IndexOf('\0');
            if (firstNull == -1) firstNull = asString.Length;
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
        /// Writes a string to a stream with the provided encoding
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="data">String data to write</param>
        /// <param name="encoding">Encoding to use</param>
        public static void WriteString(this Stream stream, string data, Encoding encoding)
        {
            var bytes = encoding.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static byte[] ReadAllBytesFrom(Stream src)
        {
            src.Rewind();
            var buffer = new byte[src.Length];
            src.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
