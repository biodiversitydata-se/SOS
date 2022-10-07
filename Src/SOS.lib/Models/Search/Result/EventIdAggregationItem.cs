using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result
{
    public class EventIdAggregationItem
    {
        public string EventId { get; set; }
        public int ObservationCount { get; set; }
    }

    public class OccurrenceIdAggregationItem
    {
        public string OccurrenceId { get; set; }
        public int ObservationCount { get; set; }
    }

    public class AggregationItem
    {
        public string AggregationKey { get; set; }
        public int DocCount { get; set; }
    }

    public class EventOccurrenceAggregationItem
    {
        public string EventId { get; set; }
        public List<string> OccurrenceIds { get; set; }        
    }
}