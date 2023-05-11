using System;
using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    public class TaxonAggregationItemDto
    {
        public DateTime? FirstSighting { get; set; }

        public DateTime? LastSighting { get; set; }
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }
    }
}
