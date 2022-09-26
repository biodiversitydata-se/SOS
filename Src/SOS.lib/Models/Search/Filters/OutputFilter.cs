using System.Collections.Generic;


namespace SOS.Lib.Models.Search.Filters
{
    public class OutputFilter
    {
        // <summary>
        /// This parameter allows you to decide what fields should be returned, using a projection.
        /// Omit this parameter and you will receive the complete collection of fields.
        /// For example, to retrieve only basic observation data, specify:
        /// ["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"]. 
        /// </summary>
        public ICollection<string> Fields { get; set; }

        /// <summary>
        /// Result sort orders
        /// </summary>
        public IEnumerable<SortOrderFilter> SortOrders { get; set; }
    }
}
