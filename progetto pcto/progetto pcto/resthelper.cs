using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace progetto_pcto
{
    class resthelper
    {
        public class Product
        {
            public string Artist { get; set; }
            public string Place { get; set; }
            public DateTime Day { get; set; }
            public string Songs { get; set; }
        }
        //apikey=TDpswW5K3jP46t26H1XXtPzZRv1xwgX2nGxo

        private static readonly string Baseurl = "https://api.setlist.fm/rest/1.0/search/setlist?artist=";
        public static string BeautyJson(string Jsonstr)
        {
            JToken parseJson = JToken.Parse(Jsonstr);
            return parseJson.ToString(Formatting.Indented);
        }
        public static async Task<string> Get(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(Baseurl + id))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
