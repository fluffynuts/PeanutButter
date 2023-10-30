using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using PeanutButter.Utils;
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
    /// Provides a simple way to run an in-memory http server situations
    /// like testing or where a small, very simple http server might be useful
    /// </summary>
    public interface IHttpServer : IHttpServerBase
    {
        /// <summary>
        /// Adds a handler to the pipeline
        /// </summary>
        /// <param name="handler"></param>
        Guid AddHandler(Func<HttpProcessor, Stream, HttpServerPipelineResult> handler);

        /// <summary>
        /// Adds a handler for providing a file download
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="contentType"></param>
        Guid AddFileHandler(
            Func<HttpProcessor, Stream, byte[]> handler,
            string contentType = MimeTypes.BYTES
        );

        /// <summary>
        /// Adds a handler to serve a text document, with (limited) automatic
        /// content-type detection.
        /// </summary>
        /// <param name="handler"></param>
        Guid AddDocumentHandler(Func<HttpProcessor, Stream, string> handler);

        /// <summary>
        /// Specifically add a handler to serve an HTML document
        /// </summary>
        /// <param name="handler"></param>
        Guid AddHtmlDocumentHandler(Func<HttpProcessor, Stream, string> handler);

        /// <summary>
        /// Specifically add a handler to serve a JSON document
        /// </summary>
        /// <param name="handler"></param>
        Guid AddJsonDocumentHandler(Func<HttpProcessor, Stream, object> handler);

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        void ServeDocument(
            string queryPath,
            XDocument doc,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        void ServeDocument(
            string queryPath,
            string doc,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        Guid ServeDocument(
            string queryPath,
            Func<string> doc,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="docFactory">Factory function to get the document contents</param>
        /// <param name="method">Which http method to respond to</param>
        Guid ServeDocument(
            string queryPath,
            Func<XDocument> docFactory,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves a JSON document with the provided data at the provided path for the
        /// provided method
        /// </summary>
        /// <param name="path">Absolute path matched for this document</param>
        /// <param name="data">Any object which will be serialized into JSON for you</param>
        /// <param name="method">Which http method to respond to</param>
        Guid ServeJsonDocument(
            string path,
            object data,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves a JSON document with the provided data at the provided path for the
        /// provided method
        /// </summary>
        /// <param name="path">Absolute path matched for this document</param>
        /// <param name="dataFactory">Factory function returning any object which will be serialized into JSON for you</param>
        /// <param name="method">Which http method to respond to</param>
        Guid ServeJsonDocument(
            string path,
            Func<object> dataFactory,
            HttpMethods method = HttpMethods.Any
        );

        /// <summary>
        /// Serves an arbitrary file from the provided path for the
        /// provided content type (defaults to application/octet-stream)
        /// </summary>
        /// <param name="path">Absolute path matched for this file</param>
        /// <param name="data">Data to provide</param>
        /// <param name="contentType">Content type of the data</param>
        Guid ServeFile(string path, byte[] data, string contentType = MimeTypes.BYTES);

        /// <summary>
        /// Serves a file via a factory Func
        /// </summary>
        /// <param name="path">Absolute path matched for this file</param>
        /// <param name="dataFactory">Factory for the data</param>
        /// <param name="contentType">Content type</param>
        Guid ServeFile(
            string path,
            Func<byte[]> dataFactory,
            string contentType = MimeTypes.BYTES
        );

        /// <summary>
        /// Clears any registered handlers &amp; log actions
        /// so the server can be re-used with completely
        /// different logic / handlers
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Provides the simple HTTP server you may use ad-hoc
    /// </summary>
    [DebuggerDisplay("HttpServer {Port} ({HandlerCount} handlers)")]
    public class HttpServer : HttpServerBase, IHttpServer
    {
        /// <summary>
        /// Used in debug display
        /// </summary>
        public int HandlerCount => _handlers.Count;

        private ConcurrentQueue<Handler> _handlers = new();

        private class Handler
        {
            public Func<HttpProcessor, Stream, HttpServerPipelineResult> Logic { get; }
            public Guid Id { get; } = Guid.NewGuid();

            public Handler(
                Func<HttpProcessor, Stream, HttpServerPipelineResult> logic
            )
            {
                Logic = logic;
                var q = new ConcurrentQueue<string>();
                q.Clear();
            }
        }

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
            {
                Start();
            }
        }

        /// <inheritdoc />
        protected override void Init()
        {
            _handlers = new();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Adds a handler to the pipeline
        /// </summary>
        /// <param name="handler"></param>
        public Guid AddHandler(Func<HttpProcessor, Stream, HttpServerPipelineResult> handler)
        {
            var container = new Handler(handler);
            _handlers.Enqueue(container);
            return container.Id;
        }

        /// <summary>
        /// Adds a handler for providing a file download
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="contentType"></param>
        public Guid AddFileHandler(
            Func<HttpProcessor, Stream, byte[]> handler,
            string contentType = MimeTypes.BYTES
        )
        {
            return AddHandler(
                (p, s) =>
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
                }
            );
        }

        /// <summary>
        /// Adds a handler to serve a text document, with (limited) automatic
        /// content-type detection.
        /// </summary>
        /// <param name="handler"></param>
        public Guid AddDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            return HandleDocumentRequestWith(
                handler,
                "auto",
                AutoSerializer,
                AutoMime
            );
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
            var end = space == -1
                ? closeTag
                : Math.Min(space, closeTag);
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
                null as string,
                (acc, cur) => acc ?? cur(trimmed)
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
        public Guid AddHtmlDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            return HandleDocumentRequestWith(
                handler,
                "html",
                null,
                _ => MimeTypes.HTML
            );
        }

        /// <summary>
        /// Specifically add a handler to serve a JSON document
        /// </summary>
        /// <param name="handler"></param>
        public Guid AddJsonDocumentHandler(Func<HttpProcessor, Stream, object> handler)
        {
            return HandleDocumentRequestWith(
                handler,
                "json",
                o => _jsonSerializer(o),
                _ => MimeTypes.JSON
            );
        }

        private Guid HandleDocumentRequestWith(
            Func<HttpProcessor, Stream, object> handler,
            string documentTypeForLogging,
            Func<object, string> stringProcessor,
            Func<string, string> mimeTypeGenerator
        )
        {
            return AddHandler(
                (p, s) =>
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
                }
            );
        }

        private void InvokeHandlersWith(HttpProcessor p, Stream stream)
        {
            var handled = false;
            foreach (var handler in _handlers)
            {
                try
                {
                    var pipelineResult = handler.Logic(p, stream);
                    if (pipelineResult == HttpServerPipelineResult.HandledExclusively)
                    {
                        handled = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var body = string.Join(
                        "\n",
                        "Internal server error",
                        ex.Message,
                        ex.StackTrace
                    );
                    Log(body);

                    p.WriteFailure(
                        HttpStatusCode.InternalServerError,
                        "Internal server error",
                        body
                    );
                    throw;
                }
            }

            if (!handled)
            {
                Log("No handlers found for {0}", p.FullUrl);
                throw new FileNotFoundException("Request was not handled by any registered handlers");
            }
        }

        /// <inheritdoc />
        public override void HandleRequestWithoutBody(HttpProcessor p, string method)
        {
            Log($"Incoming {method} request: {p.FullUrl}");
            InvokeHandlersWith(p, null);
        }

        /// <inheritdoc />
        public override void HandleRequestWithBody(HttpProcessor p, MemoryStream inputData, string method)
        {
            Log($"Incoming {method} request: {p.FullUrl}");
            InvokeHandlersWith(p, inputData);
        }

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        public void ServeDocument(
            string queryPath,
            XDocument doc,
            HttpMethods method = HttpMethods.Any
        )
        {
            ServeDocument(queryPath, () => doc.ToString(), method);
        }

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        public void ServeDocument(
            string queryPath,
            string doc,
            HttpMethods method = HttpMethods.Any
        )
        {
            ServeDocument(queryPath, () => doc, method);
        }

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="doc">XDocument to serve</param>
        /// <param name="method">Which http method to respond to</param>
        public Guid ServeDocument(
            string queryPath,
            Func<string> doc,
            HttpMethods method = HttpMethods.Any
        )
        {
            return AddHtmlDocumentHandler(
                (p, _) =>
                {
                    if (p.FullPath != queryPath || !method.Matches(p.Method))
                        return null;
                    Log("Serving html document at {0}", p.FullUrl);
                    return doc();
                }
            );
        }

        /// <summary>
        /// Serves an XDocument from the provided path, for the provided method
        /// </summary>
        /// <param name="queryPath">Absolute path to serve the document for</param>
        /// <param name="docFactory">Factory function to get the document contents</param>
        /// <param name="method">Which http method to respond to</param>
        public Guid ServeDocument(
            string queryPath,
            Func<XDocument> docFactory,
            HttpMethods method = HttpMethods.Any
        )
        {
            return ServeDocument(
                queryPath,
                () => docFactory().ToString(),
                method
            );
        }

        /// <summary>
        /// Serves a JSON document with the provided data at the provided path for the
        /// provided method
        /// </summary>
        /// <param name="path">Absolute path matched for this document</param>
        /// <param name="data">Any object which will be serialized into JSON for you</param>
        /// <param name="method">Which http method to respond to</param>
        public Guid ServeJsonDocument(
            string path,
            object data,
            HttpMethods method = HttpMethods.Any
        )
        {
            return ServeJsonDocument(path, () => data, method);
        }

        /// <summary>
        /// Serves a JSON document with the provided data at the provided path for the
        /// provided method
        /// </summary>
        /// <param name="path">Absolute path matched for this document</param>
        /// <param name="dataFactory">Factory function returning any object which will be serialized into JSON for you</param>
        /// <param name="method">Which http method to respond to</param>
        public Guid ServeJsonDocument(
            string path,
            Func<object> dataFactory,
            HttpMethods method = HttpMethods.Any
        )
        {
            return AddJsonDocumentHandler(
                (p, _) =>
                {
                    if (p.FullPath != path || !method.Matches(p.Method))
                        return null;
                    Log("Serving JSON document at {0}", p.FullUrl);
                    return dataFactory();
                }
            );
        }

        /// <summary>
        /// Serves an arbitrary file from the provided path for the
        /// provided content type (defaults to application/octet-stream)
        /// </summary>
        /// <param name="path">Absolute path matched for this file</param>
        /// <param name="data">Data to provide</param>
        /// <param name="contentType">Content type of the data</param>
        public Guid ServeFile(string path, byte[] data, string contentType = MimeTypes.BYTES)
        {
            return ServeFile(path, () => data, contentType);
        }

        /// <summary>
        /// Serves a file via a factory Func
        /// </summary>
        /// <param name="path">Absolute path matched for this file</param>
        /// <param name="dataFactory">Factory for the data</param>
        /// <param name="contentType">Content type</param>
        public Guid ServeFile(
            string path,
            Func<byte[]> dataFactory,
            string contentType = MimeTypes.BYTES
        )
        {
            return AddFileHandler(
                (p, _) =>
                {
                    if (p.Path != path)
                    {
                        return null;
                    }

                    Log("Serving file at {0}", p.FullUrl);
                    return dataFactory();
                },
                contentType
            );
        }

        /// <summary>
        /// Clears any registered handlers &amp; log actions
        /// so the server can be re-used with completely
        /// different logic / handlers
        /// </summary>
        public void Reset()
        {
            LogAction = null;
            RequestLogAction = null;
            _handlers.Clear();
        }

        /// <summary>
        /// Removes a previously-registered handler by it's id
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool RemoveHandler(Guid identifier)
        {
            var newHandlers = new ConcurrentQueue<Handler>();
            var removed = false;
            foreach (var item in _handlers.ToArray())
            {
                if (item.Id == identifier)
                {
                    removed = true;
                    continue;
                }

                newHandlers.Enqueue(item);
            }
            Interlocked.Exchange(ref _handlers, newHandlers);
            return removed;
        }
    }
}