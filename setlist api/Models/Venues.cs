using System.Collections.Generic;
using Newtonsoft.Json;

namespace SetlistNet.Models
{
    [JsonObject]
    /// <summary>
    /// Un risultato costituito da un elenco di sedi
    /// </summary>
    public class Venues : ApiArrayResult<Venue>
    {
        /// <summary>
        /// Gets o sets l'eleenco delle sedi
        /// </summary>
        [JsonProperty(PropertyName = "venue")]
        internal List<Venue> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Count = {0}", Items == null ? 0 : Items.Count);
        }
    }
}
