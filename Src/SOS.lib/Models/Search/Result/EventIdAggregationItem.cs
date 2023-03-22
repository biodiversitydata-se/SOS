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

    public class AggregationResult<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Records { get; set; }
    }

    public class AggregationItemList<TKey, TValue>
    {
        public TKey AggregationKey { get; set; }
        public List<TValue> Items { get; set; }        
    }

    public class EventOccurrenceAggregationItem
    {
        public string EventId { get; set; }
        public List<string> OccurrenceIds { get; set; }        
    }
}