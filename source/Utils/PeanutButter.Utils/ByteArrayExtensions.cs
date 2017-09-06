using System.Linq;
using System.Text;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides some extensions useful for byte arrays
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public 
#endif
        static class ByteArrayExtensions
    {
        /// <summary>
        /// Calculates the md5sum for the provided binary data
        /// </summary>
        /// <param name="data">Binary data to hash</param>
        /// <returns>hex-encoded md5sum for the provided data</returns>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public static string ToMD5String(this byte[] data)
        {
            if (data == null)
                return null;
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(data);

            var characters = hash.Select(t => t.ToString("X2")).ToList();
            return string.Join(string.Empty, characters.ToArray());
        }

        /// <summary>
        /// Provides a UTF-8 encoded string from the given binary data
        /// </summary>
        /// <param name="data">Binary data to encode as a UTF-8 string</param>
        /// <returns>The string representation of the binary data</returns>
        // ReSharper disable once InconsistentNaming
        public static string ToUTF8String(this byte[] data)
        {
            if (data == null) return null;
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Provides a base64 encoding of the given data. Basically a wrapper around
        /// System.Convert.ToBase64String
        /// </summary>
        /// <param name="data">data to encode</param>
        /// <returns>base64 representation</returns>
        public static string ToBase64(this byte[] data)
        {
            return System.Convert.ToBase64String(data);
        }
    }
}
