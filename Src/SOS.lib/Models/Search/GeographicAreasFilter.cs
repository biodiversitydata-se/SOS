using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class GeographicAreasFilter
    {
        /// <summary>
        /// Bird validation area id's where user has extended authorization
        /// </summary>
        public ICollection<string> BirdValidationAreaIds { get; set; }

        /// <summary>
        /// County id's where user has extended authorization
        /// </summary>
        public ICollection<string> CountyIds { get; set; }

        /// <summary>
        /// Other geometries where user has extended authorization
        /// </summary>
        public GeographicsFilter GeometryFilter { get; set; }

        /// <summary>
        /// Municipality id's where user has extended authorization
        /// </summary>
        public ICollection<string> MunicipalityIds { get; set; }

        /// <summary>
        /// Parish id's where user has extended authorization
        /// </summary>
        public ICollection<string> ParishIds { get; set; }

        /// <summary>
        /// Province id's where user has extended authorization
        /// </summary>
        public ICollection<string> ProvinceIds { get; set; }
    }
}