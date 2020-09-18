using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Dtos
{
    public class TaxonAggregationItemDto
    {
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }
    }
}
