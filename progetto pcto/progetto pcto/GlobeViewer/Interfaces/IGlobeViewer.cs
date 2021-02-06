using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    public struct Coordinates
    {
        public string X { get; set; }
        public string Y { get; set; }
        static Random random = new Random();

        public Coordinates(string x, string y)
        {
            X = x;
            Y = y;
        }

        public static Coordinates Random()
        {
            string latitude = random.Next(-90, 90).ToString();
            string longitude = random.Next(-180, 180).ToString();
            return new Coordinates(longitude, latitude);
        }
    }

    public interface IGlobeViewer : IDisposable
    {
        void LoadMarkers(IList<string> locations);
        void LoadMarkers(IDictionary<string, Coordinates> locations);
        void BindMarkerClickedEvent(MarkerClickedEventHandler procedure);
    }
}
