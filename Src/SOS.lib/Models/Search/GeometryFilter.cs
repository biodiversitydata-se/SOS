using System;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Class used for geometry searches
    /// </summary>
    public class GeometryFilter
    {
        /// <summary>
        /// Add buffer to geometry
        /// </summary>
        public int? Accuracy { get; set; }

        /// <summary>
        /// Point or polygon geometry used for search
        /// If point and accuracy equals 0, only sightings with exact position will be return
        /// If point and accuracy is greater tha 0, sightings inside circle (center point + buffer (accuracy)) will be returned
        /// If polygon, sightings inside polygon will be returned
        /// </summary>
        public InputGeometry Geometry { get; set; }

        public bool IsValid => (Geometry?.IsValid ?? false) && 
                               (Geometry.Type.Equals("Point", StringComparison.CurrentCultureIgnoreCase) && Accuracy > 0 ||
                                Geometry.Type.Equals("Polygon", StringComparison.CurrentCultureIgnoreCase) && Accuracy >= 0);

        /// <summary>
        /// If true, use Point accuracy when searching
        /// </summary>
        public bool UsePointAccuracy { get; set; }
    }
}
