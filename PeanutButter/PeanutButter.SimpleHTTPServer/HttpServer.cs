using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Linq;

namespace PeanutButter.SimpleHTTPServer 
{
    public enum HttpServerPipelineResult
    {
        HandledExclusively,
        Handled,
        NotHandled
    }
    // TODO: allow easier way to throw 404 (or other web exception) from simple handlers (file/document handlers)
    public class HttpServer : HttpServerBase 
    {
        private List<Func<HttpProcessor, Stream, HttpServerPipelineResult>> _handlers;

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

        public void AddHandler(Func<HttpProcessor, Stream, HttpServerPipelineResult> handler)
        {
            _handlers.Add(handler);
        }

        public string GetFullUrlFor(string relativeUrl)
        {
            var joinWith = relativeUrl.StartsWith("/") ? "" : "/";
            return String.Join(joinWith, new[] { BaseUrl, relativeUrl });
        }

        public string BaseUrl { get { return String.Format("http://localhost:{0}", this.Port); } }

        public void AddFileHandler(Func<HttpProcessor, Stream, byte[]> handler, string contentType = "application/octet-stream")
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
            AddHandler((p, s) =>
                {
                    Log("Handling document request: {0}", p.FullUrl);
                    var doc = handler(p, s);
                    if (doc == null)
                    {
                        Log(" --> no document handler set up for {0}", p.FullUrl);
                        return HttpServerPipelineResult.NotHandled;
                    }
                    p.WriteDocument(doc);
                    Log(" --> Successful document handling for {0}", p.FullUrl);
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
                try
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
                catch
                {
                }
            }
            if (!handled)
            {
                Log("No handlers found for {0}", p.FullUrl);
                throw new Exception("Request was not handled by any registered handlers");
            }
        }

        public override void HandlePOSTRequest(HttpProcessor p, Stream inputData)
        {
            Log("Incoming POST request: {0}", p.FullUrl);
            InvokeHandlersWith(p, inputData);
        }

        public void ServeDocument(string path, XDocument doc)
        {
            AddDocumentHandler((p, s) =>
                {
                    if (p.Path != path)
                        return null;
                    Log("Serving document at {0}", p.FullUrl);
                    return doc.ToString();
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


