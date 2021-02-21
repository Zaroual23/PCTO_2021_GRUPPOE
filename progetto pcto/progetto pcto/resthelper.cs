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
        private static readonly string Baseurl = "https://reqres.in/api/";
        public static string BeautyJson(string Jsonstr)
        {
            JToken parseJson = JToken.Parse(Jsonstr);
            return parseJson.ToString(Formatting.Indented);
        }
        public static async Task<string> Get(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(Baseurl + "users/" + id))
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
