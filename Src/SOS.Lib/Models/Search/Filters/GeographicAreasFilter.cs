using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class GeographicAreasFilter
    {
        /// <summary>
        /// Bird validation area id's where user has extended authorization
        /// </summary>
        public List<string> BirdValidationAreaIds { get; set; }

        /// <summary>
        /// Country region id's
        /// </summary>
        public List<string> CountryRegionIds { get; set; }

        /// <summary>
        /// County id's where user has extended authorization
        /// </summary>
        public List<string> CountyIds { get; set; }

        /// <summary>
        /// Other geometries where user has extended authorization
        /// </summary>
        public GeographicsFilter GeometryFilter { get; set; }

        /// <summary>
        /// Municipality id's where user has extended authorization
        /// </summary>
        public List<string> MunicipalityIds { get; set; }

        /// <summary>
        /// Parish id's where user has extended authorization
        /// </summary>
        public List<string> ParishIds { get; set; }

        /// <summary>
        /// Province id's where user has extended authorization
        /// </summary>
        public List<string> ProvinceIds { get; set; }
    }
}