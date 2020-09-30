using System.Collections.Generic;
using Nest;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Geometry filter.
    /// </summary>
    public class GeometryFilterDto
    {
        /// <summary>
        ///     Add buffer to geometry
        /// </summary>
        public double? MaxDistanceFromPoint { get; set; }

        /// <summary>
        ///     Point or polygon geometry used for search
        ///     If point and accuracy equals 0, only sightings with exact position will be return
        ///     If point and accuracy is greater tha 0, sightings inside circle (center point + buffer (accuracy)) will be returned
        ///     If polygon, sightings inside polygon will be returned
        /// </summary>
        public ICollection<IGeoShape> Geometries { get; set; }

        /// <summary>
        ///     If true, use Point accuracy when searching
        /// </summary>
        public bool UsePointAccuracy { get; set; }
    }
}