using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
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
            if (data is null)
            {
                return null;
            }

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
            return data is null
                ? null
                : Encoding.UTF8.GetString(data);
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

        /// <summary>
        /// Converts a byte array to memory stream
        /// - treats null like empty array
        /// </summary>
        /// <param name="bytes">input bytes</param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(
            this byte[] bytes)
        {
            return new MemoryStream(bytes ?? new byte[0]);
        }


    }
}