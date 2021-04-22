using Newtonsoft.Json;

namespace SetlistNet.Models
{
    public class Tour
    {
        private string _name;

        /// <summary>
        /// Gets o sets nome delle citta,  a seconda della lingua i valori validi sono ad es. "Mchen" o "Monaco di Baviera".
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public Tour()
        {
        }

        public Tour(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "Name = " + Name;
        }
    }
}
