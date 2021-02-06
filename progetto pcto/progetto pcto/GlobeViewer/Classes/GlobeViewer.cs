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
    public class GlobeViewer : IGlobeViewer
    {
        private readonly ChromiumWebBrowser browser = default(ChromiumWebBrowser);
        private readonly IJavascriptIntegrationService javascriptIntegrationService = default(IJavascriptIntegrationService);
        private readonly IGeocoder geocoder = default(IGeocoder);
        private readonly IHttpServer httpServer = default(IHttpServer);
        private const string publicOpenglobusApplicationUrl = "http://lmschool.altervista.org/SaveConcert";
        private bool disposed;

        public GlobeViewer(Panel panel)
        {

            string openglobusApplicationUrl = default(string);

            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                httpServer = new HttpServer();
                openglobusApplicationUrl = "http://localhost:" + httpServer.Port.ToString();
            }
            else
            {
                openglobusApplicationUrl = publicOpenglobusApplicationUrl;
            }

            browser = new ChromiumWebBrowser(openglobusApplicationUrl)
            {
                Dock = DockStyle.Fill,
            };

            javascriptIntegrationService = new JavascriptIntegrationService(browser);

            geocoder = new Geocoder();


            panel.Controls.Add(browser);
        }

        public void LoadMarkers(IList<string> locations)
        {
            IDictionary<string, Coordinates> geocodedLocations = geocoder.Geocode(locations);
            LoadMarkers(geocodedLocations);
        }

        public void LoadMarkers(IDictionary<string, Coordinates> locations)
        {
            string markersList = "[";

            foreach (var location in locations)
            {
                markersList += "{name: '";
                markersList += location.Key;
                markersList += "', latitude: ";
                markersList += location.Value.Y;
                markersList += ", longitude: ";
                markersList += location.Value.X;
                markersList += "}, ";
            }

            markersList += "]";

            javascriptIntegrationService.CallProcedure("window.loadMarkers(" + markersList + ")");
        }

        public void BindMarkerClickedEvent(MarkerClickedEventHandler procedure)
        {
            javascriptIntegrationService.MarkerClicked += procedure;
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
