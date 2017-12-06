using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace PeanutButter.SimpleHTTPServer
{
    public enum HttpServerPipelineResult
    {
        HandledExclusively,
        Handled,
        NotHandled
    }

    public enum HttpMethods
    {
        Any,
        Get,
        Post,
        Put,
        Delete,
        Patch,
        Options,
        Head
    }

    // TODO: allow easier way to throw 404 (or other web exception) from simple handlers (file/document handlers)
    public class HttpServer : HttpServerBase
    {
        private List<Func<HttpProcessor, Stream, HttpServerPipelineResult>> _handlers;

        private readonly Func<object, string> _jsonSerializer = o => JsonConvert.SerializeObject(o);

        public HttpServer(int port, bool autoStart = true, Action<string> logAction = null)
            : base(port)
        {
            LogAction = logAction;
            AutoStart(autoStart);
        }

        public HttpServer(bool autoStart = true, Action<string> logAction = null)
        {
            LogAction = logAction;
            AutoStart(autoStart);
        }

        private void AutoStart(bool autoStart)
        {
            if (autoStart)
                Start();
        }

        protected override void Init()
        {
            _handlers = new List<Func<HttpProcessor, Stream, HttpServerPipelineResult>>();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void AddHandler(Func<HttpProcessor, Stream, HttpServerPipelineResult> handler)
        {
            _handlers.Add(handler);
        }

        public string GetFullUrlFor(string relativeUrl)
        {
            var joinWith = relativeUrl.StartsWith("/") ? string.Empty : "/";
            return string.Join(joinWith, BaseUrl, relativeUrl);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string BaseUrl => $"http://localhost:{Port}";

        public void AddFileHandler(Func<HttpProcessor, Stream, byte[]> handler,
            string contentType = HttpConstants.MIMETYPE_BYTES)
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

        public void AddDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            HandleDocumentRequestWith(handler, "auto",
                AutoSerializer, AutoMime);
        }

        private static readonly Func<string, string>[] _autoMimeStrategies =
        {
            AutoMimeHtml,
            AutoMimeJson,
            AutoMimeXml,
            AutoMimeDefault
        };

        private static string AutoMimeDefault(string arg)
        {
            return HttpConstants.MIMETYPE_BYTES;
        }

        private static string AutoMimeJson(string content)
        {
            return
                content != null &&
                content.StartsWith("{") &&
                content.EndsWith("}")
                    ? HttpConstants.MIMETYPE_JSON
                    : null;
        }

        private static string AutoMimeXml(string content)
        {
            return content != null &&
                   (content.StartsWith("<?xml") || HasOpenAndCloseTags(content))
                ? HttpConstants.MIMETYPE_XML
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
                        ? HttpConstants.MIMETYPE_HTML
                        : null;
        }

        private string AutoMime(string servedResult)
        {
            var trimmed = servedResult.Trim();
            return _autoMimeStrategies.Aggregate(
                null as string, (acc, cur) => acc ?? cur(trimmed)
            );
        }

        private string AutoSerializer(object servedResult)
        {
            if (servedResult is string stringResult)
                return stringResult;
            return JsonConvert.SerializeObject(servedResult);
        }

        public void AddHtmlDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            HandleDocumentRequestWith(handler, "html", null, s => HttpConstants.MIMETYPE_HTML);
        }

        public void AddJsonDocumentHandler(Func<HttpProcessor, Stream, object> handler)
        {
            HandleDocumentRequestWith(handler, "json", o => _jsonSerializer(o), s => HttpConstants.MIMETYPE_JSON);
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

        public override void HandlePOSTRequest(HttpProcessor p, Stream inputData)
        {
            Log("Incoming POST request: {0}", p.FullUrl);
            InvokeHandlersWith(p, inputData);
        }

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

        public void ServeFile(string path, byte[] data, string contentType = "application/octet-stream")
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