﻿using NetTopologySuite.Geometries;
using SOS.Lib.Models.Gis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    ///     Class used for geometry searches
    /// </summary>
    public class GeographicsFilter
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public GeographicsFilter()
        {
            Geometries = new List<Geometry>();
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
        public List<Geometry> Geometries { get; set; }

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
                                (geom.OgcGeometryType.Equals(OgcGeometryType.Point) &&
                                 MaxDistanceFromPoint > 0.0 ||
                                 new[] { OgcGeometryType.Polygon, OgcGeometryType.MultiPolygon }.Contains(geom.OgcGeometryType));
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