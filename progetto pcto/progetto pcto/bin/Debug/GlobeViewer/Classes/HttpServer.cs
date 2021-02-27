using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace GlobeViewer.Classes
{
    class HttpServer : IHttpServer
    {
        public ushort Port { get; }

        private static IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>() {
            {".css", "text/css"},
            {".html", "text/html"},
            {".js", "application/x-javascript"},
            {".png", "image/png"},
         };

        private static string rootDirectory = @"../../../GlobeViewer/OpenglobusApplication";
        private Thread thread = default(Thread);
        private HttpListener httpListener = default(HttpListener);

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        public HttpServer()
        {
            Port = GetAvailablePort();
            thread = new Thread(Listen);
            thread.Start();
        }

        /// <summary>
        /// Get a suitable port.
        /// </summary>
        private ushort GetAvailablePort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            ushort port = (ushort)((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        private void Listen()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:" + Port.ToString() + "/");
            httpListener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = httpListener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename))
            {
                filename = "index.html";
            }

            filename = Path.Combine(rootDirectory, filename);

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    context.Response.ContentType = mimeTypeMappings[Path.GetExtension(filename)];
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }

        public void Stop()
        {
            thread.Abort();
            httpListener.Stop();
        }
    }
}
