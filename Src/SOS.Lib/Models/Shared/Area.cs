﻿using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class Area : IEntity<string>
    {
        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        public Area(AreaType areaType, string featureId)
        {
            AreaType = areaType;
            FeatureId = featureId;
            Id = AreaType.ToAreaId(featureId);
        }

        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType AreaType { get; set; }

        public LatLonBoundingBox BoundingBox { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Area Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }
}