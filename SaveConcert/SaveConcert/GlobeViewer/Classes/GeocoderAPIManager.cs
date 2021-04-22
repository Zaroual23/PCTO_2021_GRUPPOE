using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GlobeViewer.Interfaces;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

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

    /// <summary>
    /// Manages the api calls to the geocoder
    /// </summary>
    class GeocoderAPIManager : IGeocoderAPIManager
    {
        private HttpWebRequest httpWebRequest = default(HttpWebRequest);
        private Uri uri = default(Uri);
        private ApiUri apiUri = default(ApiUri);

        private DateTime lastQuery;
        private IDictionary<string, (string x, string y)> cachedCoordinates;
        private readonly string cacheFileName = "GeocoderCache";
        private readonly char cacheFileSeparator = ';';

        /// <summary>
        /// Interval in seconds between api calls
        /// </summary>
        public int RequestInterval { get; set; }

        /// <summary>
        /// Event triggered when api call is available
        /// </summary>
        public event Action ApiCallAvailable;

        private bool disposed;


        public GeocoderAPIManager(int requestInterval=0)
        {
            RequestInterval = requestInterval;

            //Initializing a dictionary used to store the cached coordinates
            cachedCoordinates = new Dictionary<string, (string, string)>();
            
            //Loading the cached coordinates from the file
            loadCachedCoordinates();
        }

        /// <summary>
        /// Geocode the given location (if the geocoder fails to retrive the coordinates, the returned state will be false and the coordinates will be set to null)
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public (bool state, string x, string y) Geocode(string location)
        {
            if ((DateTime.Now - lastQuery).Seconds <= RequestInterval)
                throw new Exception("Api call unavailable, request interval set to " + RequestInterval + " seconds beetween each call");

            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("'location' argument can't be null");

            string x = null;
            string y = null;
            bool state = false;

            //Try to geocode the location
            if (cachedCoordinates.Keys.Contains(location))
            {
                //If the location to geocode is contained in the cache, the cached values will be used to minimize the amount of api requests
                x = cachedCoordinates[location].x;
                y = cachedCoordinates[location].y;
                state = true;
            }
            else
            {
                //If the location to geocode is not contained in the cache, the coordinates will be retrived from the geocoder api
                
                //Prepare and send the api request
                apiUri.Query = location;
                uri = new Uri(apiUri.ToString());
                httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.UserAgent = "SaveConcert";
                WebResponse webResponse = httpWebRequest.GetResponse();

                //Interprer the api response
                string apiResponse = "";
                using (var sr = new System.IO.StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    apiResponse = sr.ReadToEnd();
                }
                apiResponse = apiResponse.Remove(0, 1);
                apiResponse = apiResponse.Remove(apiResponse.Length - 1, 1);

                //If the api response is valid the return values are set accordingly and are stored in the cache
                if (!string.IsNullOrEmpty(apiResponse))
                {
                    var deserializedApiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(apiResponse);
                    y = deserializedApiResponse.lat;
                    x = deserializedApiResponse.lon;
                    state = true;
                    cachedCoordinates[location] = (x, y);
                }

                //Starting the api request interval timer
                lastQuery = DateTime.Now;
                Task.Run(() => apiCallAvailableCountdown());
            }
            return (state, x, y);
        }

        private void apiCallAvailableCountdown()
        {
            //Wait for the api call interval
            Thread.Sleep(RequestInterval * 1000);

            //Trigger the event to inform that the api can now be called
            ApiCallAvailable?.Invoke();
        }

        private void cacheCoordinates()
        {
            //Save coordinates to file
            IList<string> lines = new List<string>();
            foreach (var i in cachedCoordinates)
            {
                lines.Add(i.Key + cacheFileSeparator + i.Value.x + cacheFileSeparator + i.Value.y);
            }
            File.WriteAllLines(cacheFileName, lines);
        }

        private void loadCachedCoordinates()
        {
            if (File.Exists(cacheFileName))
            {
                //Read coordinates from file
                IEnumerable<string> lines = File.ReadLines(cacheFileName);
                foreach (string line in lines)
                {
                    string[] values = new string[3];
                    values = line.Split(cacheFileSeparator);
                    cachedCoordinates[values[0]] = (values[1], values[2]);
                }
            }
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
                    cacheCoordinates();
                }
            }

            disposed = true;
        }

        ~GeocoderAPIManager()
        {
            Dispose(false);
        }
    }
}
