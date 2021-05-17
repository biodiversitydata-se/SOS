using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Class used for geometry searches
    /// </summary>
    public class GeometryFilter
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public GeometryFilter()
        {
            Geometries = new List<IGeoShape>();
        }

        /// <summary>
        /// Limit the search by a bounding box.
        /// The coordinate list should be in the format, topleft-longitude, topleft-latitude, bottomright-longitude, bottomright-latitude
        /// </summary>
        public LatLonBoundingBox BoundingBox { get; set; }

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
        /// Makes a simple validation of the geometry
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!Geometries?.Any() ?? true)
                {
                    return false;
                }
                foreach (var geom in Geometries)
                {
                    var valid = geom != null &&
                                (geom.Type.Equals("Point", StringComparison.CurrentCultureIgnoreCase) &&
                                 MaxDistanceFromPoint > 0.0 ||
                                 new[] {"polygon", "multipolygon"}.Contains(geom.Type.ToLower()));
                    if (!valid)
                    {
                        return valid;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// If true, use Point accuracy when searching
        /// </summary>
        public bool UsePointAccuracy { get; set; }

        /// <summary>
        /// If true use disturbance radius when searching
        /// </summary>
        public bool UseDisturbanceRadius { get; set; }
    }
}