using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using GlobeViewer.Interfaces;

namespace GlobeViewer.Classes
{
    /// <summary>
    /// "GlobeViewer" application main class
    /// </summary>
    public class GlobeViewer : IGlobeViewer
    {
        private readonly ChromiumWebBrowser browser = default(ChromiumWebBrowser);
        private readonly IJavascriptIntegrationService javascriptIntegrationService = default(IJavascriptIntegrationService);
        private readonly IHttpServer httpServer = default(IHttpServer);
        private readonly IGeocoderAPIManager geocoder = default(IGeocoderAPIManager);
        private const string publicOpenglobusApplicationUrl = "http://lmschool.altervista.org/SaveConcert";
        private bool disposed;

        /// <summary>
        /// Initializes the "GlobeViewer" application
        /// </summary>
        /// <param name="panel">Panel used as a container for the "ChromiumWebBrowser" CefSharp control</param>
        public GlobeViewer(Panel panel)
        {
            if (panel == null)
                throw new ArgumentException("'panel' argument can't be null");

            string openglobusApplicationUrl = default(string);

            //Getting a suitable HTTP server running the "OpenglobusApplication" 
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                //If the program is running as Administrator, an HTTP server will be created and will host the "OpenglobusApplication" locally
                httpServer = new HttpServer();
                openglobusApplicationUrl = "http://localhost:" + httpServer.Port.ToString();
            }
            else
            {
                //If the program is not running as Administrator, a remote HTTP server hosting the "OpenglobusApplication" will be used instead
                openglobusApplicationUrl = publicOpenglobusApplicationUrl;
            }

            //Initializing the browser that will present the "OpenglobusApplication"
            browser = new ChromiumWebBrowser(openglobusApplicationUrl)
            {
                Dock = DockStyle.Fill,
            };

            //Initializing the Javascript integration service for communication between .NET and Javascript 
            javascriptIntegrationService = new JavascriptIntegrationService(browser);

            //Initializing the geocoder API manager
            geocoder = new GeocoderAPIManager(requestInterval: 5);


            panel.Controls.Add(browser);
        }

        /// <summary>
        /// Load markers on the globe
        /// </summary>
        /// <param name="locations">List of tuples each one cointaining name and coordinates for a new marker</param>
        public void LoadMarkers(IList<(string Name, string X, string Y)> locations)
        {
            List<(string Name, string X, string Y)> currentlyLoadedMarkers = new List<(string Name, string X, string Y)>();

            //Preparing the parameter required by the "loadMarkers" Javascript procedure
            string markersList = "[";

            foreach (var location in locations)
            {
                string latitude = location.Y;
                string longitude = location.X;

                //Try to geocode the location to get more accurate coordinates
                try
                {
                    (bool state, string x, string y) result = geocoder.Geocode(location.Name);
                    if (result.state)
                    {
                        longitude = result.x;
                        latitude = result.y;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                if (double.Parse(longitude.Replace('.', ',')) < -180 || double.Parse(longitude.Replace('.', ',')) > 180)
                    throw new ArgumentOutOfRangeException("X (longitude) should be a value between -180 and 180");
                if (double.Parse(latitude.Replace('.', ',')) < -90 || double.Parse(latitude.Replace('.', ',')) > 90)
                    throw new ArgumentOutOfRangeException("Y (latitude) should be a value between -90 and 90");

                if (currentlyLoadedMarkers.Contains(location))
                    continue;

                markersList += "{name: '";
                markersList += location.Name;
                markersList += "', latitude: ";
                markersList += latitude;
                markersList += ", longitude: ";
                markersList += longitude;
                markersList += "}, ";

                currentlyLoadedMarkers.Add(location);
            }

            markersList += "]";

            //Finally call the Javascript procedure
            javascriptIntegrationService.CallProcedure("window.loadMarkers(" + markersList + ")");
        }

        /// <summary>
        /// Bind event handler to handle a "MarkerClickedEvent"
        /// </summary>
        /// <param name="procedure">delegate with a void (object, string) signature</param>
        public void BindMarkerClickedEvent(MarkerClickedEventHandler procedure)
        {
            javascriptIntegrationService.MarkerClicked += procedure ?? throw new ArgumentException("'procedure' argument can't be null"); ;
        }

        /// <summary>
        /// Bind event handler to handle a "ApiCallAvailableEvent"
        /// </summary>
        /// <param name="procedure">delegate with a void () signature</param>
        public void BindApiCallAvailableEvent(Action procedure)
        {
            geocoder.ApiCallAvailable += procedure ?? throw new ArgumentException("'procedure' argument can't be null");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                //Stopping the HTTP server if not null, so as not to leave running processes when the main process is closed
                httpServer?.Stop();
            }

            disposed = true;
        }

        ~GlobeViewer()
        {
            Dispose(false);
        }
    }
}
