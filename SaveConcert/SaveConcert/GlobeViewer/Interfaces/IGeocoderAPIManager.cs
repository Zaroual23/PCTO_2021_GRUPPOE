using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    /// <summary>
    /// Manages the api calls to the geocoder
    /// </summary>
    interface IGeocoderAPIManager : IDisposable
    {
        /// <summary>
        /// Interval in seconds between api calls
        /// </summary>
        int RequestInterval { get; set; }

        /// <summary>
        /// Event triggered when api call is available
        /// </summary>
        event Action ApiCallAvailable;

        /// <summary>
        /// Geocode the given location (if the geocoder fails to retrive the coordinates, the returned state will be false and the coordinates will be set to null)
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        (bool state, string x, string y) Geocode(string location);
    }
}
