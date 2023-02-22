using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Search filter.
    /// </summary>
    [DataContract]
    public class DatasetFilter
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
        /// Gets or Sets Datum
        /// </summary>
        [DataMember(Name = "datum")]
        public DateFilter DateFilter { get; set; }

        /// <summary>
        /// Gets or Sets Taxon
        /// </summary>
        [DataMember(Name = "taxon")]
        public TaxonFilter Taxon { get; set; }

        /// <summary>
        /// Gets or Sets Area
        /// </summary>
        [DataMember(Name = "area")]
        public GeographicsFilter Area { get; set; }
    }        
}
