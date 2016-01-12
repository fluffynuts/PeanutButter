/*
 * Many thanks to BobJanova for a seed project for this library (see the original here: http://www.codeproject.com/Articles/25050/Embedded-NET-HTTP-Server)
 * Original license is CPOL. My preferred licensing is BSD, which differs only from CPOL in that CPOL explicitly grants you freedom
 * from prosecution for patent infringement (not that this code is patented or that I even believe in the concept). So, CPOL it is.
 * You can find the CPOL here:
 * http://www.codeproject.com/info/cpol10.aspx 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using PeanutButter.SimpleTcpServer;

namespace PeanutButter.SimpleHTTPServer
{
    public class TcpIOWrapper : IDisposable
    {
        public Stream RawStream { get { return GetRawStream(); } }
        public StreamWriter StreamWriter { get { return GetStreamWriter(); } }

        private StreamWriter _outputStreamWriter;
        private TcpClient _client;
        private BufferedStream _rawStream;

        public TcpIOWrapper(TcpClient client)
        {
            _client = client;
        }

        public void Dispose()
        {
            lock (this)
            {
                DisposeStreamWriter();
                DisposeRawStream();
                ShutdownClient();
            }
        }

        private void ShutdownClient()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        private void DisposeStreamWriter()
        {
            if (_outputStreamWriter != null)
            {
                _outputStreamWriter.Flush();
                _outputStreamWriter.Dispose();
                _outputStreamWriter = null;
            }
        }

        private void DisposeRawStream()
        {
            if (_rawStream != null)
            {
                try
                {
                    _rawStream.Flush();
                    _rawStream.Dispose();
                }
                catch { }
                _rawStream = null;
            }
        }

        private StreamWriter GetStreamWriter()
        {
            lock (this)
            {
                if (_client == null)
                    return null;
                if (_outputStreamWriter == null)
                    _outputStreamWriter = new StreamWriter(RawStream);
                return _outputStreamWriter;
            }
        }

        private Stream GetRawStream()
        {
            lock (this)
            {
                if (_client == null) return null;
                if (_rawStream == null)
                    _rawStream = new BufferedStream(_client.GetStream());
                return _rawStream;
            }
        }
    }

    public class HttpProcessor : TcpServerProcessor, IProcessor
    {
        private const int BUF_SIZE = 4096;
        private const string CONTENT_LENGTH_HEADER = "Content-Length";
        private const string CONTENT_TYPE_HEADER = "Content-Type";
        private const string METHOD_GET = "GET";
        private const string METHOD_POST = "POST";
        private const string MIMETYPE_HTML = "text/html";
        private const int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpServerBase Server { get; protected set; }

        private StreamWriter _outputStream;

        public string Method { get; private set; }
        public string FullUrl { get; private set; }
        public string Path { get; private set; }
        public string Protocol { get; private set; }
        public Dictionary<string, string> UrlParameters { get; set; }
        public Dictionary<string, string> HttpHeaders { get; private set; }


        public HttpProcessor(TcpClient tcpClient, HttpServerBase server) : base(tcpClient)
        {
            Server = server;                   
            HttpHeaders = new Dictionary<string, string>();
        }

        public void ProcessRequest() 
        {
            using (var io = new TcpIOWrapper(TcpClient))
            {
                try
                {
                    _outputStream = io.StreamWriter;
                    ParseRequest();
                    ReadHeaders();
                    HandleRequest(io);
                }
                catch (Exception ex)
                {
                    WriteFailure();
                    Debug.WriteLine("Unable to process request: " + ex.Message);
                }
                finally
                {
                    _outputStream = null;
                }
            }
        }

        private void HandleRequest(TcpIOWrapper io)
        {
            if (Method.Equals(METHOD_GET))
                HandleGETRequest();
            else if (Method.Equals(METHOD_POST))
                HandlePOSTRequest(io.RawStream);
        }

        public void ParseRequest() 
        {
            var request = TcpClient.ReadLine();
            var tokens = request.Split(' ');
            if (tokens.Length != 3) 
            {
                throw new Exception("invalid http request line");
            }
            Method = tokens[0].ToUpper();
            FullUrl = tokens[1];
            var parts = FullUrl.Split('?');
            if (parts.Length == 1)
                UrlParameters = new Dictionary<string, string>();
            else
            {
                var all = string.Join("?", parts.Skip(1));
                UrlParameters = EncodedStringToDictionary(all);
            }
            Path = parts.First();
            Protocol = tokens[2];
        }

        private Dictionary<string, string> EncodedStringToDictionary(string s)
        {
            var parts = s.Split('&');
            return parts.Select(p =>
                                {
                                    var subParts = p.Split('=');
                                    var key = subParts.First();
                                    var value = string.Join("=", subParts.Skip(1));
                                    return new { key = key, value = value };
                                }).ToDictionary(x => x.key, x => x.value);
        }

        public void ReadHeaders() 
        {
            string line;
            while ((line = TcpClient.ReadLine()) != null) {
                if (line.Equals("")) {
                    return;
                }
                
                var separator = line.IndexOf(':');
                if (separator == -1) {
                    throw new Exception("invalid http header line: " + line);
                }
                var name = line.Substring(0, separator);
                var pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' ')) {
                    pos++; // strip any spaces
                }
                    
                var value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        public void HandleGETRequest() 
        {
            Server.HandleGETRequest(this);
        }

        public void HandlePOSTRequest(Stream stream) 
        {
            using (var ms = new MemoryStream())
            {
                if (HttpHeaders.ContainsKey(CONTENT_LENGTH_HEADER))
                {
                    var contentLength = Convert.ToInt32(HttpHeaders[CONTENT_LENGTH_HEADER]);
                    if (contentLength > MAX_POST_SIZE)
                    {
                        throw new Exception(
                            string.Format("POST Content-Length({0}) too big for this simple server",
                                          contentLength));
                    }
                    var buf = new byte[BUF_SIZE];
                    var toRead = contentLength;
                    while (toRead > 0)
                    {
                        var numread = stream.Read(buf, 0, Math.Min(BUF_SIZE, toRead));
                        if (numread == 0)
                        {
                            if (toRead == 0)
                            {
                                break;
                            }
                            else
                            {
                                throw new Exception("client disconnected during post");
                            }
                        }
                        toRead -= numread;
                        ms.Write(buf, 0, numread);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                }
                ParseFormElementsIfRequired(ms);
                Server.HandlePOSTRequest(this, ms);
            }

        }

        private void ParseFormElementsIfRequired(MemoryStream ms)
        {
            if (!HttpHeaders.ContainsKey(CONTENT_TYPE_HEADER)) return;
            if (HttpHeaders[CONTENT_TYPE_HEADER] != "application/x-www-form-urlencoded") return;
            try
            {
                var formData = Encoding.UTF8.GetString(ms.ToArray());
                FormData = EncodedStringToDictionary(formData);
            }
            catch
            {
            }
        }

        public Dictionary<string, string> FormData { get; set; }

        public void WriteSuccess(string mimeType=MIMETYPE_HTML, byte[] data = null) 
        {
            WriteOKStatusHeader();
            WriteMIMETypeHeader(mimeType);
            WriteConnectionClosesAfterCommsHeader();
            if (data != null)
            {
                WriteContentLengthHeader(data.Length);
            }
            WriteEmptyLineToStream();
            WriteDataToStream(data);
        }

        public void WriteMIMETypeHeader(string mimeType)
        {
            WriteResponseLine(CONTENT_TYPE_HEADER + ": " + mimeType);
        }

        public void WriteOKStatusHeader()
        {
            WriteStatusHeader(200, "OK");
        }

        public void WriteContentLengthHeader(int length)
        {
            WriteHeader("Content-Length", length);
        }

        public void WriteConnectionClosesAfterCommsHeader()
        {
            WriteHeader("Connection", "close");
        }

        public void WriteHeader(string header, string value)
        {
            WriteResponseLine(string.Join(": ", new[] { header, value }));
        }

        public void WriteHeader(string header, int value)
        {
            WriteHeader(header, value.ToString());
        }

        public void WriteStatusHeader(int errorCode, string message)
        {
            WriteResponseLine(string.Join(" ", new[] { "HTTP/1.0", errorCode.ToString(), message }));
        }

        public void WriteDataToStream(byte[] data)
        {
            if (data == null) return;
            _outputStream.Flush();
            _outputStream.BaseStream.Write(data, 0, data.Length);
            _outputStream.BaseStream.Flush();
        }

        public void WriteFailure() {
            WriteStatusHeader(404, "File not found");
            WriteConnectionClosesAfterCommsHeader();
            WriteEmptyLineToStream();
        }

        public void WriteEmptyLineToStream()
        {
            WriteResponseLine("");
        }

        public void WriteResponseLine(string response)
        {
            _outputStream.WriteLine(response);
        }

        public void WriteDocument(string document, string mimeType = MIMETYPE_HTML)
        {
            WriteSuccess(MIMETYPE_HTML, Encoding.UTF8.GetBytes(document));
        }
    }

}