using System.Collections.Generic;
using Newtonsoft.Json;

namespace SetlistNet.Models
{
    [JsonObject]
    /// <summary>
    /// Il risultato è un elenco di paesi
    /// </summary>
    public class Countries : ApiArrayResult<Country>
    {
        /// <summary>
        /// Gets o sets l'elenco dei paesi.
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        internal List<Country> Items
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
