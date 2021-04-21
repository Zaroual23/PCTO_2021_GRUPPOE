using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    interface IGeocoderAPIManager
    {
        (bool state, string x, string y) Geocode(string location);
    }
}
