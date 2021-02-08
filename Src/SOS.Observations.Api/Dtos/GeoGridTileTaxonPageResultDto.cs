using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    public class GeoGridTileTaxonPageResultDto
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
        public IEnumerable<GeoGridTileTaxaCellDto> GridCells { get; set; }
    }
}