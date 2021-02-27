using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    /// <summary>
    /// HTTP server for Javascript CORS requests handling, requires administrator privileges
    /// </summary>
    interface IHttpServer
    {
        /// <summary>
        /// Available port found and used by the HTTP server (readonly)
        /// </summary>
        ushort Port { get; }

        /// <summary>
        /// Stops the HTTP server, running process is left when not called
        /// </summary>
        void Stop();
    }
}
