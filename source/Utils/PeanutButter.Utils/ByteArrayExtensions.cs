﻿using System;
using System.IO;
using System.IO.Compression;
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
        [Obsolete($"renamed to {nameof(ToMd5String)}")]
        public static string ToMD5String(
            this byte[] data
        )
        {
            return data.ToMd5String();
        }

        /// <summary>
        /// Calculates the md5sum for the provided binary data
        /// </summary>
        /// <param name="data">Binary data to hash</param>
        /// <returns>hex-encoded md5sum for the provided data</returns>
        public static string ToMd5String(this byte[] data)
        {
            if (data is null)
            {
                return null;
            }

            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(data);

            var characters = hash.Select(t => t.ToString("X2")).ToArray();
            return string.Join(string.Empty, characters);
        }

        /// <summary>
        /// Provides a UTF-8 encoded string from the given binary data
        /// </summary>
        /// <param name="data">Binary data to encode as a UTF-8 string</param>
        /// <returns>The string representation of the binary data</returns>
        // ReSharper disable once InconsistentNaming
        [Obsolete($"renamed to {nameof(ToUtf8String)}")]
        public static string ToUTF8String(this byte[] data)
        {
            return data.ToUtf8String();
        }

        /// <summary>
        /// Provides a UTF-8 encoded string from the given binary data
        /// </summary>
        /// <param name="data">Binary data to encode as a UTF-8 string</param>
        /// <returns>The string representation of the binary data</returns>
        // ReSharper disable once InconsistentNaming
        public static string ToUtf8String(
            this byte[] data
        )
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
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Converts a byte array to memory stream
        /// - treats null like empty array
        /// </summary>
        /// <param name="bytes">input bytes</param>
        /// <returns>MemoryStream wrapping input bytes</returns>
        public static MemoryStream ToMemoryStream(
            this byte[] bytes
        )
        {
            return new MemoryStream(bytes ?? new byte[0]);
        }

#if NETSTANDARD
        /// <summary>
        /// </summary>
        /// <param name="bytes">input bytes</param>
        /// <returns>ArraySegment wrapping input bytes</returns>
        public static ArraySegment<byte> ToArraySegment(
            this byte[] bytes
        )
        {
            return new ArraySegment<byte>(bytes);
        }
#endif

        /// <summary>
        /// Tests if the byte array being operated on starts
        /// with the bytes in the reference array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool StartsWith(
            this byte[] data,
            byte[] reference
        )
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (reference is null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            return data.Take(reference.Length)
                .SequenceEqual(reference);
        }

        /// <summary>
        /// GZips the input array, producing a new array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GZip(this byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            
            using var source = new MemoryStream(data);
            using var target = new MemoryStream();
            using var gzip = new GZipStream(
                target,
                CompressionLevel.Optimal,
                leaveOpen: true
            );
            source.CopyTo(gzip);
            gzip.Close();
            return target.ToArray();
        }

        /// <summary>
        /// Decompresses the array of gzipped data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] UnGZip(this byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using var source = new MemoryStream(data);
            using var target = new MemoryStream();
            using var gzip = new GZipStream(
                source,
                CompressionMode.Decompress,
                leaveOpen: true
            );
            gzip.CopyTo(target);
            gzip.Close();
            return target.ToArray();
        }

        /// <summary>
        /// Indicates whether a byte array has been gzipped
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsGZipped(
            this byte[] data
        )
        {
            return data is not null && 
                data.Length >= GZipHeader.Length &&
                data.StartsWith(GZipHeader);
        }

        private static readonly byte[] GZipHeader =
        [
            0x1f,
            0x8b,
            8,
            0,
            0,
            0,
            0,
            0
        ];
    }
}