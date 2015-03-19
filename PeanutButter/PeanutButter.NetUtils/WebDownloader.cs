using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.Utils;

namespace PeanutButter.NetUtils
{
    public class WebDownloader
    {
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
                var chunkSize = 8192;

                var thisChunk = new byte[chunkSize];
                while (haveRead < expectedLength)
                {
                    var toRead = expectedLength - haveRead;
                    if (toRead > chunkSize)
                        toRead = chunkSize;
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
