using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Location related filter
    /// </summary>
    public class LocationFilter
    {
        /// <summary>
        /// Geographical areas to filter by
        /// </summary>
        public IEnumerable<AreaFilter> Areas { get; set; }

        /// <summary>
        /// Filter on area geometries 
        /// </summary>
        public GeographicAreasFilter AreaGeographic { get; set; }

        /// <summary>
        /// Geometry filter
        /// </summary>
        public GeographicsFilter Geometries { get; set; }

        /// <summary>
        /// Filter on location id's
        /// </summary>
        public IEnumerable<string> LocationIds { get; set; }

        /// <summary>
        /// Limit observation accuracy. Only observations with accuracy less than this will be returned
        /// </summary>
        public int? MaxAccuracy { get; set; }

        /// <summary>
        /// Location name wild card filter
        /// </summary>
        public string NameFilter { get; set; }
    }
}
