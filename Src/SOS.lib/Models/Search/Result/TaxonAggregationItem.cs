using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result
{
    public class TaxonAggregationHit
    {
        public DateTime EventStartDate { get; set; }

        public string OccurrenceId { get; set; }
    }

    public class TaxonAggregationItem
    {
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }

        public DateTime? FirstSighting { get; set; }

        public DateTime? LastSighting { get; set; }

        public IEnumerable<TaxonAggregationHit> LatestObservations { get; set; }

        public static TaxonAggregationItem Create(int taxonId, int count, DateTime? firstSighting, DateTime? lastSighting, IEnumerable<TaxonAggregationHit> latestObservations)
        {
            return new TaxonAggregationItem
            {
                FirstSighting = firstSighting,
                LastSighting = lastSighting,
                TaxonId = taxonId,
                ObservationCount = count,
                LatestObservations = latestObservations
            };
        }
    }
}