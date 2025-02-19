﻿using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class GridCellDto
    {
        /// <summary>
        /// Grid cell Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Cell coordinates in WGS84
        /// </summary>
        public LatLonBoundingBoxDto BoundingBox { get; set; }

        /// <summary>
        /// Number of observations in cell
        /// </summary>
        public long? ObservationsCount { get; set; }

        /// <summary>
        /// Cell coordinates in SWEREF99 TM
        /// </summary>
        public XYBoundingBoxDto MetricBoundingBox { get; set; }

        /// <summary>
        /// Count of different taxa
        /// </summary>
        public long? TaxaCount { get; set; }
    }
}