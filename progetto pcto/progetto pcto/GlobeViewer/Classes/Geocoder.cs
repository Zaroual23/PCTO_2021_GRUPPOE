using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;

namespace GlobeViewer.Classes
{
    class Geocoder : IGeocoder
    {
        private IDictionary<string, Coordinates> knownCoordinates;

        public Geocoder()
        {
            knownCoordinates = new Dictionary<string, Coordinates>();
        }

        public IDictionary<string, Coordinates> Geocode(IList<string> locations)
        {
            IDictionary<string, Coordinates> returnCoordinates = new Dictionary<string, Coordinates>();
            foreach (string location in locations)
            {
                if (knownCoordinates.Keys.Contains(location))
                    returnCoordinates.Add(location, knownCoordinates[location]);
                else
                {
                    Coordinates newCoordinates = Geocode(location);
                    returnCoordinates.Add(location, newCoordinates);
                    knownCoordinates.Add(location, newCoordinates);
                }

            }

            return returnCoordinates;
        }

        private Coordinates Geocode(string location)
        {
            Coordinates newCoordinates = Coordinates.Random();
            return newCoordinates;
        }
    }
}
