using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides some extensions for the stream object
    /// provided into a HttpServer handler function
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Attempt to read the stream and deserialize JSON
        /// to an object of type T. If T is 'string', you
        /// get the string back. Uses the UTF8 encoding for
        /// reading a string.
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T As<T>(
            this Stream stream)
        {
            return stream.As<T>(Encoding.UTF8);
        }

        /// <summary>
        /// Attempt to read the stream and deserialize
        /// to an object of type T. If T is 'string', you
        /// get the string back.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T As<T>(
            this Stream stream, Encoding encoding)
        {
            stream?.TryRewind();
            var bytes = stream?.ReadAllBytes() ?? new byte[0];
            var str = encoding.GetString(bytes);
            if (typeof(T) == typeof(string))
            {
                return (T)(object)str;
            }
            return JsonConvert.DeserializeObject<T>(str);
        }

        private static byte[] ReadAllBytes(this Stream stream)
        {
            var memStream = new MemoryStream();
            var buffer = new byte[32768];
            var read = 0;
            do
            {
                read = stream.Read(buffer, 0, buffer.Length);
                memStream.Write(buffer, 0, read);
            } while (read > 0);
            return memStream.ToArray();
        }

        private static bool TryRewind(this Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}