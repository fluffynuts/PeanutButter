using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream src)
        {
            return src == null
                    ? null
                    : ReadAllBytesFrom(src);
        }

        public static void WriteAllBytes(this Stream src, byte[] data)
        {
            if (src == null)
                throw new IOException("Source stream is null");
            if ((data ?? new byte[] { }).Length == 0)
                return;
            src.Seek(0, SeekOrigin.Begin);
            src.SetLength(0);
            src.Write(data, 0, data.Length);
        }

        public static void Rewind(this Stream src)
        {
            src.Seek(0, SeekOrigin.Begin);
        }

        public static void Append(this Stream dst, byte[] data)
        {
            if (dst == null)
                throw new IOException("Target stream is null");
            dst.Seek(0, SeekOrigin.End);
            dst.Write(data, 0, data.Length);
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
