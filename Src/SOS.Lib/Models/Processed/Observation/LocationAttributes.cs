﻿using SOS.Lib.Swagger;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Location attributes.
    /// </summary>
    public class LocationAttributes
    {
        /// <summary>
        ///     Special handling of Kalmar/Öland.
        /// </summary>
        public string CountyPartIdByCoordinate { get; set; }

        /// <summary>
        ///     External Id of site
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// True if it's a private location
        /// </summary>
        [SwaggerExclude]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Id of project
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        ///     Spacial handling of Lappland.
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
