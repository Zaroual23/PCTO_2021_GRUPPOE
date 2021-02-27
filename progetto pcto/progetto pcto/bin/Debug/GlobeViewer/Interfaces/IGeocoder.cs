using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    interface IGeocoder
    {
        IDictionary<string, Coordinates> Geocode(IList<string> locations);
    }
}
