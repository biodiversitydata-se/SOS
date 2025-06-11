﻿
using NetTopologySuite.Geometries;
using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Geometry filter.
    /// </summary>
    public class GeographicsFilterDto
    {
        /// <summary>
        /// Area filter
        /// </summary>
        public IEnumerable<AreaFilterDto>? Areas { get; set; }

        /// <summary>
        /// Limit the search by a bounding box.
        /// </summary>
        public LatLonBoundingBoxDto? BoundingBox { get; set; }

        /// <summary>
        /// If true, observations that are outside Geometries polygons
        /// but close enough when disturbance sensitivity of species
        /// are considered, will be included in the result.
        /// </summary>
        public bool ConsiderDisturbanceRadius { get; set; } = false;

        /// <summary>
        /// If true, observations that are outside Geometries polygons
        /// but possibly inside when accuracy (coordinateUncertaintyInMeters)
        /// of observation is considered, will be included in the result.
        /// </summary>
        public bool ConsiderObservationAccuracy { get; set; } = false;

        /// <summary>
        /// If Geometries is of point type, this property must be set to a value greater than 0.
        /// Observations inside circle (center=point, radius=MaxDistanceFromPoint) will be returned.
        /// </summary>
        public double? MaxDistanceFromPoint { get; set; }

        /// <summary>
        /// Point or polygon geometry used for search.
        /// If the geometry is a point, then MaxDistanceFromPoint is also used in search.
        /// </summary>
        public ICollection<Geometry> Geometries { get; set; }

        /// <summary>
        /// Filter on location id/s. Only observations with passed location id/s this will be returned
        /// </summary>
        public IEnumerable<string>? LocationIds { get; set; }

        /// <summary>
        /// Location name wild card filter
        /// </summary>
        public string? LocationNameFilter { get; set; }

        /// <summary>
        /// Limit observation accuracy. Only observations with accuracy less than this will be returned
        /// </summary>
        public int? MaxAccuracy { get; set; }

        /// <summary>
        /// Indicates that the geometry is invalid.
        /// </summary>
        [JsonIgnore]
        public bool IsGeometryInvalid { get; set; }
    }
}