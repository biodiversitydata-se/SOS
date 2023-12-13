using System;
using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos
{
    public class TaxonAggregationItemDto
    {
        public DateTime? FirstSighting { get; set; }

        public DateTime? LastSighting { get; set; }
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }
    }
}
