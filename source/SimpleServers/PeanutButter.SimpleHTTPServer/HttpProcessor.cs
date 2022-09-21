/*
 * Many thanks to BobJanova for a seed project for this library (see the original here: http://www.codeproject.com/Articles/25050/Embedded-NET-HTTP-Server)
 * Original license is CPOL. My preferred licensing is BSD, which differs only from CPOL in that CPOL explicitly grants you freedom
 * from prosecution for patent infringement (not that this code is patented or that I even believe in the concept). So, CPOL it is.
 * You can find the CPOL here:
 * http://www.codeproject.com/info/cpol10.aspx 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PeanutButter.SimpleTcpServer;
using PeanutButter.Utils;
using static PeanutButter.SimpleHTTPServer.HttpConstants;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InconsistentNaming

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Processor for HTTP requests on top of the generic TCP processor
    /// </summary>
    public class HttpProcessor : TcpServerProcessor, IProcessor
    {
        /// <summary>
        /// Action to use when attempting to log arbitrary data
        /// </summary>
        public Action<string> LogAction => Server.LogAction;

        /// <summary>
        /// Action to use when attempting to log requests
        /// </summary>
        public Action<RequestLogItem> RequestLogAction => Server.RequestLogAction;

        private const int BUF_SIZE = 4096;

        /// <summary>
        /// Provides access to the server associated with this processor
        /// </summary>
        public HttpServerBase Server { get; protected set; }

        private StreamWriter _outputStream;

        /// <summary>
        /// Method of the current request being processed
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Full url for the request being processed
        /// </summary>
        public string FullUrl { get; private set; }

        /// <summary>
        /// Just the path for the request being processed
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Protocol for the request being processed
        /// </summary>
        public string Protocol { get; private set; }

        /// <summary>
        /// Url parameters for the request being processed
        /// </summary>
        public Dictionary<string, string> UrlParameters { get; set; }

        /// <summary>
        /// Headers on the request being processed
        /// </summary>
        public Dictionary<string, string> HttpHeaders { get; private set; }

        /// <summary>
        /// Maximum size, in bytes, to accept for a POST
        /// </summary>
        public long MaxPostSize { get; set; } = MAX_POST_SIZE;


        /// <inheritdoc />
        public HttpProcessor(TcpClient tcpClient, HttpServerBase server) : base(tcpClient)
        {
            Server = server;
            HttpHeaders = new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public void ProcessRequest()
        {
            using (var io = new TcpIoWrapper(TcpClient))
            {
                try
                {
                    _outputStream = io.StreamWriter;
                    ParseRequest();
                    ReadHeaders();
                    HandleRequest(io);
                }
                catch (FileNotFoundException)
                {
                    WriteFailure(HttpStatusCode.NotFound, Statuses.NOTFOUND);
                }
                catch (Exception ex)
                {
                    WriteFailure(HttpStatusCode.InternalServerError, $"{Statuses.INTERNALERROR}: {ex.Message}");
                    LogAction?.Invoke("Unable to process request: " + ex.Message);
                }
                finally
                {
                    _outputStream = null;
                }
            }
        }

        /// <summary>
        /// Handles the request, given an IO wrapper
        /// </summary>
        /// <param name="io"></param>
        protected void HandleRequest(TcpIoWrapper io)
        {
            if (Method.Equals(Methods.GET))
            {
                this.HandleRequestWithoutBody(Method);
                return;
            }

            if (Method.Equals(Methods.PUT)
                || Method.Equals(Methods.DELETE)
                || Method.Equals(Methods.PATCH)
                || Method.Equals(Methods.POST))
            {
                this.HandleRequestWithBody(io.RawStream, Method);
            }
        }

        /// <summary>
        /// Parses the request from the TcpClient
        /// </summary>
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
                return new { key, value };
            }).ToDictionary(x => x.key, x => x.value);
        }

        /// <summary>
        /// Reads in the headers from the TcpClient
        /// </summary>
        public void ReadHeaders()
        {
            string line;
            while ((line = TcpClient.ReadLine()) != null)
            {
                if (line.Equals(string.Empty))
                {
                    return;
                }

                var separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }

                var name = line.Substring(0, separator);
                var pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                var value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        /// <summary>
        /// Handles a request that does not contain a body (as of the HTTP spec).
        /// </summary>
        private void HandleRequestWithoutBody(string method)
        {
            Server.HandleRequestWithoutBody(this, method);
        }

        private void HandleRequestWithBody(Stream stream, string method)
        {
            using var ms = new MemoryStream();
            if (HttpHeaders.ContainsKey(Headers.CONTENT_LENGTH))
            {
                var contentLength = Convert.ToInt32(HttpHeaders[Headers.CONTENT_LENGTH]);
                ReadBodyContent(stream, method, contentLength, ms);
            }
            else if (HttpHeaders.ContainsKey(Headers.TRANSFER_ENCODING))
            {
                var transferEncoding = HttpHeaders[Headers.TRANSFER_ENCODING];
                if (!transferEncoding.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException(
                        $"Transfer-Encoding '{transferEncoding}' is not supported"
                    );
                }

                ReadChunked(stream, method, ms);
            }

            ParseFormElementsIfRequired(ms);
            Server.HandleRequestWithBody(this, ms, method);
        }

        // see: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Transfer-Encoding#chunked_encoding
        private void ReadChunked(
            Stream stream,
            string method,
            MemoryStream ms
        )
        {
            var eol = new byte[2];
            while (true)
            {
                var chunkSize = stream.ReadLine();
                if (chunkSize is null)
                {
                    return;
                }

                if (!chunkSize.TryParseHex(out var toRead))
                {
                    throw new InvalidOperationException(
                        $"{method} Expected a chunk size, but found '{chunkSize}'"
                    );
                }

                if (toRead == 0)
                {
                    // this is the terminating chunk - we're done after reading the
                    // final trailing newline
                    ReadNewlineFromStream();
                    return;
                }

                CopyBytes(stream, toRead, ms);
                ReadNewlineFromStream();
            }

            void ReadNewlineFromStream()
            {
                // skip the trailing \r\n
                var eolRead = stream.Read(eol, 0, 2);
                if (eolRead != 2 || eol[0] != '\r' || eol[1] != '\n')
                {
                    LogAction?.Invoke(
                        $"WARNING: possibly invalid chunked transfer: expected [ 13, 10 ], but received [ {eol[0]}, {eol[1]} ]"
                    );
                }
            }
        }

        private void ReadBodyContent(Stream stream, string method, int contentLength, MemoryStream ms)
        {
            if (contentLength > MaxPostSize)
            {
                throw new Exception(
                    $"{method} Content-Length({contentLength}) too big for this simple server (max: {MaxPostSize})"
                );
            }

            CopyBytes(stream, contentLength, ms);

            ms.Seek(0, SeekOrigin.Begin);
        }

        private static void CopyBytes(
            Stream source,
            int howMany,
            Stream target
        )
        {
            var buf = new byte[BUF_SIZE];
            var toRead = howMany;
            while (toRead > 0)
            {
                var numRead = source.Read(buf, 0, Math.Min(BUF_SIZE, toRead));
                if (numRead == 0)
                {
                    throw new Exception("client disconnected during post");
                }

                toRead -= numRead;
                target.Write(buf, 0, numRead);
            }
        }

        /// <summary>
        /// Parses form data on the request, if available
        /// </summary>
        /// <param name="ms"></param>
        public void ParseFormElementsIfRequired(MemoryStream ms)
        {
            if (!HttpHeaders.ContainsKey(Headers.CONTENT_TYPE)) return;
            if (HttpHeaders[Headers.CONTENT_TYPE] != "application/x-www-form-urlencoded") return;
            try
            {
                var formData = Encoding.UTF8.GetString(ms.ToArray());
                FormData = EncodedStringToDictionary(formData);
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        /// <summary>
        /// Form data associated with the request
        /// </summary>
        public Dictionary<string, string> FormData { get; set; }

        /// <summary>
        /// Perform a successful write (ie, HTTP status 200) with the optionally
        /// provided mime type and data data
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="data"></param>
        public void WriteSuccess(string mimeType = MimeTypes.HTML, byte[] data = null)
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

        /// <summary>
        /// Writes the specified MIME header (Content-Type)
        /// </summary>
        /// <param name="mimeType"></param>
        public void WriteMIMETypeHeader(string mimeType)
        {
            WriteResponseLine(Headers.CONTENT_TYPE + ": " + mimeType);
        }

        /// <summary>
        /// Writes the OK status header
        /// </summary>
        public void WriteOKStatusHeader()
        {
            WriteStatusHeader(HttpStatusCode.OK, "OK");
        }

        /// <summary>
        /// Writes the Content-Length header with the provided length
        /// </summary>
        /// <param name="length"></param>
        public void WriteContentLengthHeader(int length)
        {
            WriteHeader("Content-Length", length);
        }

        /// <summary>
        /// Writes the header informing the client that the connection
        /// will not be held open
        /// </summary>
        public void WriteConnectionClosesAfterCommsHeader()
        {
            WriteHeader("Connection", "close");
        }

        /// <summary>
        /// Writes an arbitrary header to the response stream
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void WriteHeader(string header, string value)
        {
            WriteResponseLine(string.Join(": ", header, value));
        }

        /// <summary>
        /// Writes an integer-value header to the response stream
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void WriteHeader(string header, int value)
        {
            WriteHeader(header, value.ToString());
        }

        /// <summary>
        /// Writes the specified status header to the response stream,
        /// with the optional message
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void WriteStatusHeader(HttpStatusCode code, string message = null)
        {
            LogRequest(code, message);
            WriteResponseLine($"HTTP/1.0 {(int) code} {message ?? code.ToString()}");
        }

        private void LogRequest(HttpStatusCode code, string message)
        {
            var action = RequestLogAction;
            action?.Invoke(
                new RequestLogItem(FullUrl, code, Method, message, HttpHeaders)
            );
        }

        /// <summary>
        /// Writes arbitrary byte data to the response stream
        /// </summary>
        /// <param name="data"></param>
        public void WriteDataToStream(byte[] data)
        {
            if (data == null) return;
            try
            {
                _outputStream.Flush();
                _outputStream.BaseStream.Write(data, 0, data.Length);
                _outputStream.BaseStream.Flush();
            }
            catch (Exception ex)
            {
                LogAction?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Writes an arbitrary string to the response stream
        /// </summary>
        /// <param name="data"></param>
        public void WriteDataToStream(string data)
        {
            WriteDataToStream(
                Encoding.UTF8.GetBytes(data ?? "")
            );
        }

        /// <summary>
        /// Writes out a simple http failure
        /// </summary>
        /// <param name="code"></param>
        public void WriteFailure(HttpStatusCode code)
        {
            WriteFailure(code, code.ToString());
        }

        /// <summary>
        /// Writes a failure code and message to the response stream and closes
        /// the response
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void WriteFailure(
            HttpStatusCode code,
            string message
        )
        {
            WriteFailure(code, message, null);
        }

        /// <summary>
        /// Writes out an http failure with body text
        /// </summary>
        /// <param name="code">The http code to return</param>
        /// <param name="message">The message to add to the status line</param>
        /// <param name="body">The body to write out</param>
        public void WriteFailure(
            HttpStatusCode code,
            string message,
            string body)
        {
            WriteFailure(code, message, body, "text/plain");
        }

        /// <summary>
        /// Write out a failure with a message, body and custom mime-type
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="body"></param>
        /// <param name="mimeType"></param>
        public void WriteFailure(
            HttpStatusCode code,
            string message,
            string body,
            string mimeType
        )
        {
            WriteStatusHeader(code, message);
            WriteConnectionClosesAfterCommsHeader();
            if (string.IsNullOrEmpty(body))
            {
                WriteEmptyLineToStream();
                return;
            }

            WriteMIMETypeHeader(mimeType);
            WriteContentLengthHeader(body.Length);
            WriteConnectionClosesAfterCommsHeader();
            WriteEmptyLineToStream();
            WriteDataToStream(body);
        }

        /// <summary>
        /// Writes an empty line to the stream: HTTP is line-based,
        /// so the client will probably interpret this as an end
        /// of section / request
        /// </summary>
        public void WriteEmptyLineToStream()
        {
            WriteResponseLine(string.Empty);
        }

        /// <summary>
        /// Write an arbitrary string response to the response stream
        /// </summary>
        /// <param name="response"></param>
        public void WriteResponseLine(string response)
        {
            _outputStream.WriteLine(response);
        }

        /// <summary>
        /// Write a textural document to the response stream with
        /// the assumption that the mime type is text/html
        /// </summary>
        /// <param name="document"></param>
        public void WriteDocument(string document)
        {
            WriteDocument(document, MimeTypes.HTML);
        }

        /// <summary>
        /// Write a textural document to the response stream with
        /// the optionally-provided mime type
        /// </summary>
        /// <param name="document"></param>
        /// <param name="mimeType"></param>
        public void WriteDocument(string document, string mimeType)
        {
            WriteSuccess(mimeType, Encoding.UTF8.GetBytes(document));
        }
    }
}