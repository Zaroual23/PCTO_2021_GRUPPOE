using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeViewer.Interfaces
{
    interface IGeocoderAPIManager
    {
        int RequestInterval { get; set; }
        event Action ApiCallAvailable;
        (bool state, string x, string y) Geocode(string location);
    }
}
