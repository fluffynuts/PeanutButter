using System;
using System.Collections.Generic;
using System.IO;
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
        Delete
    }

    // TODO: allow easier way to throw 404 (or other web exception) from simple handlers (file/document handlers)
    public class HttpServer : HttpServerBase 
    {
        private List<Func<HttpProcessor, Stream, HttpServerPipelineResult>> _handlers;

        public Func<object, string> JsonSerializer = o => JsonConvert.SerializeObject(o);

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

        public void AddFileHandler(Func<HttpProcessor, Stream, byte[]> handler, string contentType = HttpConstants.MIMETYPE_BYTES)
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
            HandleDocumentRequestWith(handler, "html", null, HttpConstants.MIMETYPE_HTML);
        }

        public void AddJsonDocumentHandler(Func<HttpProcessor, Stream, object> handler)
        {
            HandleDocumentRequestWith(handler, "json", o => JsonSerializer(o), HttpConstants.MIMETYPE_JSON);
        }

        private void HandleDocumentRequestWith(Func<HttpProcessor, Stream, object> handler, 
                                                string documentTypeForLogging, 
                                                Func<object, string> stringProcessor, 
                                                string mimeType)
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
                p.WriteDocument(asString, mimeType);
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
                switch (pipelineResult)
                {
                    case HttpServerPipelineResult.Handled:
                        // TODO: allow more pipelining
                    case HttpServerPipelineResult.HandledExclusively:
                        handled = true;
                        break;
                }
                if (pipelineResult == HttpServerPipelineResult.HandledExclusively)
                    break;
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

        public void ServeDocument(string path, XDocument doc, HttpMethods method = HttpMethods.Any)
        {
            AddDocumentHandler((p, s) =>
                {
                    if (p.Path != path || !method.Matches(p.Method))
                        return null;
                    Log("Serving html document at {0}", p.FullUrl);
                    return doc.ToString();
                });
        }

        public void ServeJsonDocument(string path, object data, HttpMethods method = HttpMethods.Any)
        {
            AddJsonDocumentHandler((p, s) =>
            {
                if (p.Path != path || !method.Matches(p.Method))
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


