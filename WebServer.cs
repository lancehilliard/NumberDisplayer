using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace NumberDisplayer.WindowsApplication {
    public interface IWebServer {
        void Run();
        void Stop();
    }

    // https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server.aspx
    public class WebServer : IWebServer {
        private readonly Action<Exception> _exceptionAction;
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        private WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method, Action<Exception> exceptionAction) {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            _responderMethod = method ?? throw new ArgumentException(nameof(method));
            _exceptionAction = exceptionAction ?? throw new ArgumentException(nameof(exceptionAction));
            if (prefixes == null || prefixes.Count == 0) throw new ArgumentException("prefixes");
            foreach (var s in prefixes) _listener.Prefixes.Add(s);
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, Action<Exception> exceptionAction, params string[] prefixes) : this(prefixes, method, exceptionAction) { }

        public void Run() {
            ThreadPool.QueueUserWorkItem(o => {
                try {
                    while (_listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c => {
                            var context = c as HttpListenerContext;
                            try {
                                if (context != null) {
                                    var rstr = _responderMethod(context.Request);
                                    var buf = Encoding.UTF8.GetBytes(rstr);
                                    context.Response.ContentLength64 = buf.Length;
                                    context.Response.OutputStream.Write(buf, 0, buf.Length);
                                }
                            }
                            catch (Exception e) {
                                _exceptionAction(e);
                            }
                            finally {
                                context?.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                }
                catch (Exception e) {
                    _exceptionAction(e);
                }
            });
        }

        public void Stop() {
            _listener.Stop();
            _listener.Close();
        }
    }
}