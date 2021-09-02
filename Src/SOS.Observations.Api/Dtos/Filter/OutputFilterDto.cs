using System.Collections.Generic;
using SOS.Lib.Enums;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Response output settings
    /// </summary>
    public class OutputFilterDto
    {
        /// <summary>
        /// Pre defined sets of output fields
        /// </summary>
        public OutputFieldSet? FieldSet { get; set; }

        // <summary>
        /// This parameter allows you to decide what fields should be returned, using a projection.
        /// Omit this parameter and you will receive the complete collection of fields.
        /// For example, to retrieve only basic observation data, specify:
        /// ["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"]. 
        /// </summary>
        public ICollection<string> Fields { get; set; }
    }
}
