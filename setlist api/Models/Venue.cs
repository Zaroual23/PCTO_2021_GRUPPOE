using Newtonsoft.Json;

namespace SetlistNet.Models
{
    /// <summary>
    /// Questa classe rappresenta un luogo. Di solito è il nome del luogo e della città messi insieme.. 
    /// <para>See remarks for more info.</para>
    /// </summary>
    /// <remarks> 
    /// I luoghi sono luoghi in cui si svolgono i concerti.
    /// Di solito sono costituiti dal nome di un luogo e da una città, ma ci sono anche alcuni luoghi a cui non è ancora associata una città.
    /// </remarks>
    public class Venue
    {
        #region Private Fields
        private string _id;
        private string _name;
        private City _city;
        private string _url;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets unique identifier.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the venue, usually without city and country. 
        /// <para>E.g. "Madison Square Garden" or "Royal Albert Hall".</para>
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
        /// <summary>
        /// Gets or sets the city in which the venue is located.
        /// </summary>
        [JsonProperty(PropertyName = "city")]
        public City City
        {
            get
            {
                return this._city;
            }
            set
            {
                this._city = value;
            }
        }
        /// <summary>
        /// Gets or sets the attribution url.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }
        #endregion

        public Venue()
        {

        }

        public Venue(string name) : this()
        {
            Name = name;
        }

        public Venue(City city) : this()
        {
            City = city;
        }

        public override string ToString()
        {
            return "Name = " + Name;
        }
    }
}
