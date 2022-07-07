using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore
{
    public static class FormFileExtensions
    {
        public static byte[] ReadAllBytes(
            this IFormFile file
        )
        {
            var target = new MemoryStream();
            using var s = file.OpenReadStream();
            s.CopyTo(target);
            return target.ToArray();
        }

        public static string ReadAllText(
            this IFormFile file
        )
        {
            return Encoding.UTF8.GetString(
                file.ReadAllBytes()
            );
        }
    }
}