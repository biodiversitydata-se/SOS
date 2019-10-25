using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Import.Models
{
    public class SpeciesPortalAggregationOptions
    {
        public int ChunkSize { get; set; } = 1000000;
        public bool AddSightingsToVerbatimDatabase { get; set; } = true;
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;
    }
}
