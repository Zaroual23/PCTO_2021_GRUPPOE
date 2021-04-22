using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;
using Newtonsoft.Json;
using System.Threading;

namespace GlobeViewer.Classes
{
    struct ApiUri
    {
        private static readonly string start_uri = "https://nominatim.openstreetmap.org/?addressdetails=1&q=";
        public string Query { get; set; }
        private static readonly string end_uri = "&format=json&limit=1";

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Query))
                throw new Exception("Search query is not set");
            return start_uri + Query + end_uri;
        }
    }

    class GeocoderAPIManager : IGeocoderAPIManager
    {
        private HttpWebRequest httpWebRequest = default(HttpWebRequest);
        private Uri uri = default(Uri);
        private ApiUri apiUri = default(ApiUri);

        private DateTime lastQuery;
        private IDictionary<string, (string, string)> cachedCoordinates;

        public int RequestInterval { get; set; }
        public event Action ApiCallAvailable;


        public GeocoderAPIManager(int requestInterval=0)
        {
            RequestInterval = requestInterval;
            cachedCoordinates = new Dictionary<string, (string, string)>();
        }

        public (bool state, string x, string y) Geocode(string location)
        {
            if ((DateTime.Now - lastQuery).Seconds <= RequestInterval)
                throw new Exception("Api call unavailable, request interval set to " + RequestInterval + " seconds beetween each call");

            string x = null;
            string y = null;
            bool state = false;

            apiUri.Query = location;
            uri = new Uri(apiUri.ToString());
            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.UserAgent = "SaveConcert";
            WebResponse webResponse = httpWebRequest.GetResponse();

            string apiResponse = "";
            using (var sr = new System.IO.StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
            {
                apiResponse = sr.ReadToEnd();
            }
            apiResponse = apiResponse.Remove(0, 1);
            apiResponse = apiResponse.Remove(apiResponse.Length - 1, 1);

            if (!string.IsNullOrEmpty(apiResponse))
            {
                var deserializedApiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(apiResponse);
                y = deserializedApiResponse.lat;
                x = deserializedApiResponse.lon;
                state = true;
            }

            lastQuery = DateTime.Now;
            Task.Run(() => apiCallAvailableCountdown());
            return (state, x, y);
        }

        private void apiCallAvailableCountdown()
        {
            Thread.Sleep(RequestInterval * 1000);
            ApiCallAvailable?.Invoke();
        }
    }
}
