﻿using System.Collections.Generic;

namespace SOS.Lib.Models.Gis
{
    public class GeoGridTileTaxonPageResult
    {
        public bool HasMorePages { get; set; }

        /// <summary>
        /// The GeoTile page key.
        /// </summary>
        public string NextGeoTilePage { get; set; }

        /// <summary>
        /// The TaxonId page key.
        /// </summary>
        public int? NextTaxonIdPage { get; set; }
        public IEnumerable<GeoGridTileTaxaCell> GridCells { get; set; }
    }
}