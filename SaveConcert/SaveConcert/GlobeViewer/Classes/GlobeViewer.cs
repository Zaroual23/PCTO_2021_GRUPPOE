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
        private bool LoadMarkersTaskRunning;
        private bool disposed;

        public delegate void LoadMarkersTaskStateSwitchedEventHandler(object sender, bool state);
        public event LoadMarkersTaskStateSwitchedEventHandler LoadMarkersTaskStateSwitched;

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
            geocoder = new GeocoderAPIManager(requestInterval: 1);


            panel.Controls.Add(browser);
        }

        /// <summary>
        /// Load markers on the globe
        /// </summary>
        /// <param name="locations">List of tuples each one cointaining name and coordinates for a new marker</param>
        public void LoadMarkers(IList<(string markerName, string Name, string X, string Y)> locations, bool geocodeAlways=false, bool skipUngeocodableLocations=false)
        {
            if (!LoadMarkersTaskRunning)
            {
                Task.Run(() =>
                {
                    LoadMarkersTaskRunning = true;
                    LoadMarkersTaskStateSwitched?.Invoke(this, true);

                    List<(string markerName, string Name, string X, string Y)> currentlyLoadedMarkers = new List<(string markerName, string Name, string X, string Y)>();

                    //Preparing the parameter required by the "loadMarkers" Javascript procedure
                    string markersList = "[";

                    foreach (var location in locations)
                    {
                        if (string.IsNullOrEmpty(location.Name))
                            throw new Exception("elements contained in 'locations' can't have null names");

                        string latitude = location.Y;
                        string longitude = location.X;

                        //Try to geocode the location if longitude or latitude is not provided or geocodeAlways is set to true
                        if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude) || geocodeAlways)
                        {
                            (bool state, string x, string y) result;
                            if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
                                result = geocoder.Geocode(location.Name, coordinatesToStoreInCache: (longitude, latitude));
                            else
                                result = geocoder.Geocode(location.Name);

                            if (result.state)
                            {
                                longitude = result.x;
                                latitude = result.y;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
                                {
                                    if (skipUngeocodableLocations)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        throw new Exception($"location '{location.Name}' could not be geocoded");
                                    }
                                }
                            }
                        }

                        if (double.Parse(longitude.Replace('.', ',')) < -180 || double.Parse(longitude.Replace('.', ',')) > 180)
                            throw new ArgumentOutOfRangeException("X (longitude) should be a value between -180 and 180");
                        if (double.Parse(latitude.Replace('.', ',')) < -90 || double.Parse(latitude.Replace('.', ',')) > 90)
                            throw new ArgumentOutOfRangeException("Y (latitude) should be a value between -90 and 90");

                        if (currentlyLoadedMarkers.Contains(location))
                            continue;

                        markersList += "{name: '";
                        markersList += location.markerName;
                        markersList += "', latitude: ";
                        markersList += latitude.Replace(',', '.');
                        markersList += ", longitude: ";
                        markersList += longitude.Replace(',', '.');
                        markersList += "}, ";

                        currentlyLoadedMarkers.Add(location);
                    }

                    markersList += "]";

                    //Finally call the Javascript procedure
                    javascriptIntegrationService.CallProcedure("window.loadMarkers(" + markersList + ")");

                    LoadMarkersTaskRunning = false;
                    LoadMarkersTaskStateSwitched?.Invoke(this, false);
                });
            }
        }

        /// <summary>
        /// Bind event handler to handle a "MarkerClickedEvent"
        /// </summary>
        /// <param name="procedure">delegate with a void (object, string) signature</param>
        public void BindMarkerClickedEvent(MarkerClickedEventHandler procedure)
        {
            javascriptIntegrationService.MarkerClicked += procedure ?? throw new ArgumentException("'procedure' argument can't be null"); ;
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

                //Disposing the geocoder if not null, to cache geocoded coordinates
                geocoder?.Dispose();
            }

            disposed = true;
        }

        ~GlobeViewer()
        {
            Dispose(false);
        }
    }
}
