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
        /// Limit the search by a bounding box.
        /// The coordinate list should be in the format, topleft-longitude, topleft-latitude, bottomright-longitude, bottomright-latitude
        /// </summary>
        public List<double> BoundingBox { get; set; }

        /// <summary>
        /// If Geometries is of point type, this property must be set to a value greater than 0.
        /// Observations inside circle (center=point, radius=MaxDistanceFromPoint) will be returned.
        /// </summary>
        public double? MaxDistanceFromPoint { get; set; }

        /// <summary>
        /// Point or polygon geometry used for search.
        /// If the geometry is a point, then MaxDistanceFromPoint is also used in search.
        /// </summary>
        public ICollection<IGeoShape> Geometries { get; set; }

        /// <summary>
        /// If true, observations that are outside Geometries polygons
        /// but possibly inside when observation taxon disturbance radius
        /// is considered, will be included in the result.
        /// </summary>
        public bool ConsiderDisturbanceRadius { get; set; }

        /// <summary>
        /// If true, observations that are outside Geometries polygons
        /// but possibly inside when accuracy (coordinateUncertaintyInMeters)
        /// of observation is considered, will be included in the result.
        /// </summary>
        public bool ConsiderObservationAccuracy { get; set; }

        /// <summary>
        /// Limit observation accuracy. Only observations with accuracy less than this will be returned
        /// </summary>
        public int? MaxAccuracy { get; set; }
    }
}