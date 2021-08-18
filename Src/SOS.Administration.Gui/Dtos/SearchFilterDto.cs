using System.Collections.Generic;

namespace SOS.Administration.Gui.Dtos
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterDto : SearchFilterBaseDto
    {
        /// <summary>
        /// This parameter allows you to decide what fields should be returned, using a projection.
        /// Omit this parameter and you will receive the complete collection of fields.
        /// For example, to retrieve only basic observation data, specify:
        /// ["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"]. 
        /// </summary>
        public ICollection<string> OutputFields { get; set; }
    }
}