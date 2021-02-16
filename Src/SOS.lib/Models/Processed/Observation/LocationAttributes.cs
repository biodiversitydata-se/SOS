namespace SOS.Lib.Models.Processed.Observation
{
    public class LocationAttributes
    {
        /// <summary>
        ///     Special handling of Kalmar/Öland
        /// </summary>
        public string CountyPartIdByCoordinate { get; set; }

        /// <summary>
        ///     Spacial handling of Lappland
        /// </summary>
        public string ProvincePartIdByCoordinate { get; set; }

        /// <summary>
        ///     The original municipality value from data provider.
        /// </summary>
        public string VerbatimMunicipality { get; set; }

        /// <summary>
        ///     The original StateProvince value from data provider.
        /// </summary>
        public string VerbatimProvince { get; set; }
    }
}
