using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    /// <summary>
    /// "GlobeViewer" application main class
    /// </summary>
    public interface IGlobeViewer : IDisposable
    {
        /// <summary>
        /// Load markers on the globe
        /// </summary>
        /// <param name="locations">List of tuples each one cointaining name and coordinates for a new marker</param>
        void LoadMarkers(IList<(string Name, string X, string Y)> locations);

        /// <summary>
        /// Bind event handler to handle a "MarkerClickedEvent"
        /// </summary>
        /// <param name="procedure">delegate with a void (object, string) signature</param>
        void BindMarkerClickedEvent(MarkerClickedEventHandler procedure);
    }
}
