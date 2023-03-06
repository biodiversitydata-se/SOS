using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Search filter.
    /// </summary>
    [DataContract]
    public class EventsFilter
    {
        /// <summary>
        /// Gets or Sets ExportMode
        /// </summary>
        [DataMember(Name = "exportMode")]
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Gets or Sets DatasetIds
        /// </summary>        
        [DataMember(Name = "datasetIds")]
        public List<string> DatasetIds { get; set; }

        /// <summary>
        /// Gets or Sets EventIds
        /// </summary>        
        [DataMember(Name = "eventIds")]
        public List<string> EventIds { get; set; }

        /// <summary>
        /// Datum
        /// </summary>
        [DataMember(Name = "datum")]
        public DateFilter DateFilter { get; set; }

        /// <summary>
        /// Taxon
        /// </summary>
        [DataMember(Name = "taxon")]
        public TaxonFilter Taxon { get; set; }

        /// <summary>
        /// Area
        /// </summary>
        [DataMember(Name = "area")]
        public GeographicsFilter Area { get; set; }
    }
}
