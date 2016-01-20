using System.Linq;
using System.Text;

namespace PeanutButter.Utils
{
    public static class ByteArrayExtensions
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public static string ToMD5String(this byte[] data)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(data);

            var characters = hash.Select(t => t.ToString("X2")).ToList();
            return string.Join(string.Empty, characters.ToArray());
        }

        // ReSharper disable once InconsistentNaming
        public static string ToUTF8String(this byte[] data)
        {
            if (data == null) return null;
            return Encoding.UTF8.GetString(data);
        }
    }
}
