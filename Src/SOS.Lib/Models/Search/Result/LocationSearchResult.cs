namespace SOS.Lib.Models.Search.Result
{
    public class LocationSearchResult
    {
        /// <summary>
        /// County
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Id of location 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Location latitude in WGS 84
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Location longitude in WGS 84
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Municipality
        /// </summary>
        public string Municipality { get; set; }

        /// <summary>
        /// Name of location 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parish
        /// </summary>
        public string Parish { get; set; }
    }
}
