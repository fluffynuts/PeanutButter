using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using static PeanutButter.SimpleHTTPServer.HttpConstants;
// ReSharper disable UnusedMember.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Result to return from one part of the pipeline
    /// </summary>
    public enum HttpServerPipelineResult
    {
        /// <summary>
        /// The request was handled exclusively and processing should stop
        /// </summary>
        HandledExclusively,

        /// <summary>
        /// The request was handled, but may continue along the pipeline
        /// </summary>
        Handled,

        /// <summary>
        /// The request was not handled at all: processing should continue
        /// </summary>
        NotHandled
    }

    /// <summary>
    /// Http methods supported by requests
    /// </summary>
    public enum HttpMethods
    {
        /// <summary>
        /// This request is for or supports any method
        /// </summary>
        Any,

        /// <summary>
        /// This request is for or supports GET
        /// </summary>
        Get,

        /// <summary>
        /// This request is for or supports POST
        /// </summary>
        Post,

        /// <summary>
        /// This request is for or supports PUT
        /// </summary>
        Put,

        /// <summary>
        /// This request is for or supports DELETE
        /// </summary>
        Delete,

        /// <summary>
        /// This request is for or supports PATCH
        /// </summary>
        Patch,

        /// <summary>
        /// This request is for or supports OPTIONS
        /// </summary>
        Options,
        /// <summary>
        /// This request is for or supports HEAD
        /// </summary>
        Head
    }

    // TODO: allow easier way to throw 404 (or other web exception) from simple handlers (file/document handlers)
    /// <summary>
    /// Provides the simple HTTP server you may use ad-hoc
    /// </summary>
    public class HttpServer : HttpServerBase
    {
        private List<Func<HttpProcessor, Stream, HttpServerPipelineResult>> _handlers;

        private readonly Func<object, string> _jsonSerializer = JsonConvert.SerializeObject;

        /// <inheritdoc />
        public HttpServer(int port, bool autoStart, Action<string> logAction)
            : base(port)
        {
            LogAction = logAction;
            AutoStart(autoStart);
        }

        /// <summary>
        /// Constructs an HttpServer listening on the configured port with the given logAction
        /// </summary>
        /// <param name="port"></param>
        /// <param name="logAction"></param>
        public HttpServer(int port, Action<string> logAction) 
            : this(port, true, logAction)
        {
        }

        /// <summary>
        /// Constructs an HttpServer with autoStart true and no logAction
        /// </summary>
        public HttpServer() : this(true, null)
        {
        }

        /// <summary>
        /// Constructs an HttpServer with autoStart true and provided logAction
        /// </summary>
        /// <param name="logAction">Action to log messages with</param>
        public HttpServer(Action<string> logAction) : this(true, logAction)
        {
        }

        /// <summary>
        /// Constructs an HttpServer based on passed in autoStart, with no logAction
        /// </summary>
        /// <param name="autoStart">Start immediately?</param>
        public HttpServer(bool autoStart) : this(autoStart, null)
        {
        }

        /// <summary>
        /// Constructs an HttpServer
        /// </summary>
        /// <param name="autoStart">Start immediately?</param>
        /// <param name="logAction">Action to log messages</param>
        public HttpServer(bool autoStart, Action<string> logAction)
        {
            LogAction = logAction;
            AutoStart(autoStart);
        }

        private void AutoStart(bool autoStart)
        {
            if (autoStart)
                Start();
        }

        /// <inheritdoc />
        protected override void Init()
        {
            _handlers = new List<Func<HttpProcessor, Stream, HttpServerPipelineResult>>();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Adds a handler to the pipeline
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(Func<HttpProcessor, Stream, HttpServerPipelineResult> handler)
        {
            _handlers.Add(handler);
        }

        /// <summary>
        /// Provides the full url, including protocol, host and port
        /// for the provided relative Url: most useful when using an
        /// automatic port
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public string GetFullUrlFor(string relativeUrl)
        {
            var joinWith = relativeUrl.StartsWith("/") ? string.Empty : "/";
            return string.Join(joinWith, BaseUrl, relativeUrl);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Provides the base url from which the server serves
        /// </summary>
        public string BaseUrl => $"http://localhost:{Port}";

        /// <summary>
        /// Adds a handler for providing a file download
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="contentType"></param>
        public void AddFileHandler(Func<HttpProcessor, Stream, byte[]> handler,
            string contentType = MimeTypes.BYTES)
        {
            AddHandler((p, s) =>
            {
                Log("Handling file request: {0}", p.FullUrl);
                var data = handler(p, s);
                if (data == null)
                {
                    Log(" --> no file handler set up for {0}", p.FullUrl);
                    return HttpServerPipelineResult.NotHandled;
                }
                p.WriteSuccess(contentType, data);
                Log(" --> Successful file handling for {0}", p.FullUrl);
                return HttpServerPipelineResult.HandledExclusively;
            });
        }

        /// <summary>
        /// Adds a handler to serve a text document, with (limited) automatic
        /// content-type detection.
        /// </summary>
        /// <param name="handler"></param>
        public void AddDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            HandleDocumentRequestWith(handler, "auto",
                AutoSerializer, AutoMime);
        }

        private static readonly Func<string, string>[] AutoMimeStrategies =
        {
            AutoMimeHtml,
            AutoMimeJson,
            AutoMimeXml,
            AutoMimeDefault
        };

        private static string AutoMimeDefault(string arg)
        {
            return MimeTypes.BYTES;
        }

        private static string AutoMimeJson(string content)
        {
            return
                content != null &&
                content.StartsWith("{") &&
                content.EndsWith("}")
                    ? MimeTypes.JSON
                    : null;
        }

        private static string AutoMimeXml(string content)
        {
            return content != null &&
                   (content.StartsWith("<?xml") || HasOpenAndCloseTags(content))
                ? MimeTypes.XML
                : null;
        }

        private static bool HasOpenAndCloseTags(string content, string tag = null)
        {
            if (!content.StartsWith("<") && !content.EndsWith(">"))
                return false;
            var openingTag = tag ?? FindOpeningTag(content);
            return content.StartsWith($"<{openingTag}", StringComparison.OrdinalIgnoreCase) &&
                   content.EndsWith($"</{openingTag}>", StringComparison.OrdinalIgnoreCase);
        }

        private static string FindOpeningTag(string content)
        {
            var openTag = content.IndexOf("<", StringComparison.Ordinal);
            var closeTag = content.IndexOf(">", openTag, StringComparison.Ordinal);
            var space = content.IndexOf(" ", openTag, StringComparison.Ordinal);
            var end = space == -1 ? closeTag : Math.Min(space, closeTag);
            return content.Substring(openTag + 1, end - openTag - 1);
        }

        private static string AutoMimeHtml(string content)
        {
            return content != null &&
                   (content.ToLowerInvariant().StartsWith("<!doctype") ||
                    HasOpenAndCloseTags(content, "html"))
                ? MimeTypes.HTML
                : null;
        }

        private string AutoMime(string servedResult)
        {
            var trimmed = servedResult.Trim();
            return AutoMimeStrategies.Aggregate(
                null as string, (acc, cur) => acc ?? cur(trimmed)
            );
        }

        private string AutoSerializer(object servedResult)
        {
            if (servedResult is string stringResult)
                return stringResult;
            return JsonConvert.SerializeObject(servedResult);
        }

        /// <summary>
        /// Specifically add a handler to serve an HTML document
        /// </summary>
        /// <param name="handler"></param>
        public void AddHtmlDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            HandleDocumentRequestWith(handler, "html", null, s => MimeTypes.HTML);
        }

        /// <summary>
        /// Specifically add a handler to serve a JSON document
        /// </summary>
        /// <param name="handler"></param>
        public void AddJsonDocumentHandler(Func<HttpProcessor, Stream, object> handler)
        {
            HandleDocumentRequestWith(handler, "json", o => _jsonSerializer(o), s => MimeTypes.JSON);
        }

        private void HandleDocumentRequestWith(Func<HttpProcessor, Stream, object> handler,
            string documentTypeForLogging,
            Func<object, string> stringProcessor,
            Func<string, string> mimeTypeGenerator)
        {
            AddHandler((p, s) =>
            {
                Log($"Handling {documentTypeForLogging} document request: {p.FullUrl}");
                var doc = handler(p, s);
                if (doc == null)
                {
                    Log($" --> no {documentTypeForLogging} document handler set up for {0}", p.FullUrl);
                    return HttpServerPipelineResult.NotHandled;
                }
                var asString = doc as string;
                if (asString == null)
                {
                    try
                    {
                        asString = stringProcessor(doc);
                    }
                    catch (Exception ex)
                    {
                        Log($"Unable to process request to string result: {ex.Message}");
                    }
                }
                p.WriteDocument(asString, mimeTypeGenerator(asString));
                Log($" --> Successful {documentTypeForLogging} document handling for {p.FullUrl}");
                return HttpServerPipelineResult.HandledExclusively;
            });
        }

        /// <inheritdoc />
        public override void HandleGETRequest(HttpProcessor p)
        {
            Log("Incoming GET request: {0}", p.FullUrl);
            InvokeHandlersWith(p, null);
        }

        private void InvokeHandlersWith(HttpProcessor p, Stream stream)
        {
            var handled = false;
            foreach (var handler in _handlers)
            {
                var pipelineResult = handler(p, stream);
                if (pipelineResult == HttpServerPipelineResult.HandledExclusively)
                {
                    handled = true;
                    break;
                }
            }
            if (!handled)
            {
                Log("No handlers found for {0}", p.FullUrl);
                throw new FileNotFoundException("Request was not handled by any registered handlers");
            }
        }

        /// <inheritdoc />
        public override void HandlePOSTRequest(HttpProcessor p, Stream inputData)
        {
            Log("Incoming POST request: {0}", p.FullUrl);
            InvokeHandlersWith(p, inputData);
        }

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath"></param>
        /// <param name="doc"></param>
        /// <param name="method"></param>
        public void ServeDocument(string queryPath, XDocument doc, HttpMethods method = HttpMethods.Any)
        {
            AddHtmlDocumentHandler((p, s) =>
            {
                if (p.FullUrl != queryPath || !method.Matches(p.Method))
                    return null;
                Log("Serving html document at {0}", p.FullUrl);
                return doc.ToString();
            });
        }

        /// <summary>
        /// Serves a JSON document with the provided data at the provided path for the
        /// provided method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="method"></param>
        public void ServeJsonDocument(string path, object data, HttpMethods method = HttpMethods.Any)
        {
            AddJsonDocumentHandler((p, s) =>
            {
                if (p.FullUrl != path || !method.Matches(p.Method))
                    return null;
                Log("Serving JSON document at {0}", p.FullUrl);
                return data;
            });
        }

        /// <summary>
        /// Serves an arbitrary file from the provided path for the
        /// provided content type (defaults to application/octet-stream)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        public void ServeFile(string path, byte[] data, string contentType = MimeTypes.BYTES)
        {
            AddFileHandler((p, s) =>
            {
                if (p.Path != path)
                    return null;
                Log("Serving file at {0}", p.FullUrl);
                return data;
            }, contentType);
        }
    }
}