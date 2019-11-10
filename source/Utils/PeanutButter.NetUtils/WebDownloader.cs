using System.IO;
using System.Net;
using System.Threading.Tasks;
using PeanutButter.Utils;

namespace PeanutButter.NetUtils
{
    public class WebDownloader
    {
        private const int DEFAULT_CHUNK_SIZE = 8192; // 8kb
        public async Task<bool> Download(string linkUrl, string outputPath)
        {
            using (var disposer = new AutoDisposer())
            {
                var req = WebRequest.Create(linkUrl);
                var response = await disposer.Add(req.GetResponseAsync());
                var readStream = disposer.Add(response.GetResponseStream());
                var writeStream = disposer.Add(new FileStream(outputPath, FileMode.CreateNew));

                var expectedLength = long.Parse(response.Headers["Content-Length"]);
                var haveRead = 0;

                var thisChunk = new byte[DEFAULT_CHUNK_SIZE];
                while (haveRead < expectedLength)
                {
                    var toRead = expectedLength - haveRead;
                    if (toRead > DEFAULT_CHUNK_SIZE)
                        toRead = DEFAULT_CHUNK_SIZE;
                    var readBytes = await readStream.ReadAsync(thisChunk, 0, (int)toRead);
                    await writeStream.WriteAsync(thisChunk, 0, readBytes);
                    writeStream.Flush();
                    haveRead += readBytes;
                }
            }
            return true;
        }
    }
}
