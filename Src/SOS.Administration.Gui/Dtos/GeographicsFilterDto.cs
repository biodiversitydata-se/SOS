using System.Collections.Generic;
using Nest;

namespace SOS.Administration.Gui.Dtos
{
    /// <summary>
    /// Geometry filter.
    /// </summary>
    public class GeographicsFilterDto
    {
        /// <summary>
        /// Area filter
        /// </summary>
        public IEnumerable<AreaFilterDto> Areas { get; set; }

        /// <summary>
        /// Limit the search by a bounding box.
        /// </summary>
        public LatLonBoundingBoxDto BoundingBox { get; set; }

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
        /// but close enough when disturbance sensitivity of species
        /// are considered, will be included in the result.
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