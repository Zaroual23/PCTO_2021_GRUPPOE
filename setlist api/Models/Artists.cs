using System.Collections.Generic;
using Newtonsoft.Json;

namespace SetlistNet.Models
{
    [JsonObject]
    /// <summary>
    /// Viene restituito un elenco di artisti
    /// </summary>
    public class Artists : ApiArrayResult<Artist>
    {
        /// <summary>
        /// Gets o sets della lista di artisti.
        /// </summary>
        [JsonProperty(PropertyName = "artist")]
        internal List<Artist> Items
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
