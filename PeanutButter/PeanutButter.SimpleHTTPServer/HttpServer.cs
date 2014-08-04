using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PeanutButter.SimpleHTTPServer 
{
    public enum HttpServerPipelineResult
    {
        HandledExclusively,
        Handled,
        NotHandled
    }
    public class HttpServer : HttpServerBase 
    {
        private readonly List<Func<HttpProcessor, Stream, HttpServerPipelineResult>> _handlers;

        public HttpServer() : this(FindOpenRandomPort())
        {
        }

        private static int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = rnd.Next(5000, 50000);
            var seekingPort = true;
            while (seekingPort)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Any, tryThis);
                    listener.Start();
                    listener.Stop();
                    seekingPort = false;
                }
                catch
                {
                    Thread.Sleep(rnd.Next(1, 50));
                }
            }
            return tryThis;
        }

        public HttpServer(int port) : base(port) 
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

        public void AddFileHandler(Func<HttpProcessor, Stream, byte[]> handler)
        {
            AddHandler((p, s) =>
                {
                    var data = handler(p, s);
                    if (data == null) return HttpServerPipelineResult.NotHandled;
                    p.WriteSuccess("application/octet-stream", data);
                    return HttpServerPipelineResult.HandledExclusively;

                });
        }

        public void AddDocumentHandler(Func<HttpProcessor, Stream, string> handler)
        {
            AddHandler((p, s) =>
                {
                    var doc = handler(p, s);
                    if (doc == null) return HttpServerPipelineResult.NotHandled;
                    p.WriteDocument(doc);
                    return HttpServerPipelineResult.HandledExclusively;
                });
        }

        public override void HandleGETRequest(HttpProcessor p)
        {
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
                throw new Exception("Request was not handled by any registered handlers");
        }

        public override void HandlePOSTRequest(HttpProcessor p, Stream inputData)
        {
            InvokeHandlersWith(p, inputData);
        }

    }


}


