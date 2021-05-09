using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetlistNet;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace SaveConcert
{
    class SCSetlistApi : SetlistApi
    {
        private DateTime lastQuery;
        public int RequestInterval { get; set; }

        public SCSetlistApi(string apiToken, int requestInterval=1) : base(apiToken)
        {
            RequestInterval = requestInterval;
        }

        public new T Load<T>(string url)
        {
            Uri uri = new Uri(root + "/rest/" + version + url);

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Accept = "application/json";
            request.Headers.Add("x-api-key:" + token);
            request.Headers.Add("Accept-Language:" + language);
            try
            {
                System.Threading.Thread.Sleep(RequestInterval * 1000 - (DateTime.Now - lastQuery).Milliseconds);
            } catch (ArgumentOutOfRangeException) { }
            var response = request.GetResponse();
            lastQuery = DateTime.Now;
            string value = "";
            using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                value = sr.ReadToEnd();
            }

            var result = JsonConvert.DeserializeObject<T>(value);
            return result;
        }
    }
}
