using System.Collections.Generic;
using Newtonsoft.Json;

namespace SetlistNet.Models
{
    [JsonObject]
    /// <summary>
    /// Si riceve una lista di città
    /// </summary>
    public class Cities : ApiArrayResult<City>
    {
        /// <summary>
        /// Gets o sets della lista di città
        /// </summary>
        [JsonProperty(PropertyName = "cities")]
        internal List<City> Items
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
