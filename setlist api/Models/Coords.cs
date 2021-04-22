using Newtonsoft.Json;

namespace SetlistNet.Models
{
    /// <summary>
    /// Vengono rappresentate le coordinate di un punto sul globo <paramref name="Cities"/>.
    /// </summary>
    public class Coords
    {
        #region Private Fields
        private double? _longitude;
        private bool _longitudeSpecified;
        private double? _latitude;
        private bool _latitudeSpecified;
        #endregion

        #region Properties
        /// <summary>
        /// Gets o sets la parte della longitudine
        /// </summary>
        [JsonProperty(PropertyName = "long")]
        public double Longitude
        {
            get
            {
                return this._longitude.GetValueOrDefault();
            }
            set
            {
                this._longitude = value;
                this._longitudeSpecified = true;
            }
        }
        /// <summary>
        /// Gets o sets se la longitudine deve essere inclusa nell'output.
        /// </summary>
        public bool LongitudeSpecified
        {
            get
            {
                return this._longitudeSpecified;
            }
            set
            {
                this._longitudeSpecified = value;
            }
        }
        /// <summary>
        /// Gets o sets la parte edlla latitudine
        /// </summary>
        [JsonProperty(PropertyName = "lat")]
        public double Latitude
        {
            get
            {
                return this._latitude.GetValueOrDefault();
            }
            set
            {
                this._latitude = value;
                this._latitudeSpecified = true;
            }
        }
        /// <summary>
        /// Gets o sets se la latitudine deve essere inclusa nell'output.
        /// </summary>
        public bool LatitudeSpecified
        {
            get
            {
                return this._latitudeSpecified;
            }
            set
            {
                this._latitudeSpecified = value;
            }
        }
        #endregion

        /// <summary>
        /// vengono restituite la longitudine e la latitudine con formato  "lat,long".
        /// </summary>
        /// <returns>lappresenta la longitudine e la latitudine separate da una virgola</returns>
        public override string ToString()
        {
            return string.Format("Latitude = {0}, Longitude = {1}", Latitude, Longitude);
        }
    }
}
