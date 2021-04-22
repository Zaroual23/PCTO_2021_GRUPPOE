using System.Collections.Generic;
using Newtonsoft.Json;

namespace SetlistNet.Models
{
    [JsonObject]
    /// <summary>
    /// Questa classe rappresenta un risultato: un elenco di setlist.
    /// </summary>
    public class Setlists : ApiArrayResult<Setlist>
    {
        /// <summary>
        /// Gets o sets elenco di setlists
        /// </summary>
        [JsonProperty(PropertyName = "setlist")]
        internal List<Setlist> Items
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
