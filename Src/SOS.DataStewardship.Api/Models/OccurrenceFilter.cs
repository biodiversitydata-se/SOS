using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Search filter.
    /// </summary>
    [DataContract]
    public class OccurrenceFilter
    {
        /// <summary>
        /// DatasetIds
        /// </summary>
        [DataMember(Name = "datasetIds")]
        public List<string> DatasetIds { get; set; }

        /// <summary>
        /// EventIds
        /// </summary>
        [DataMember(Name = "eventIds")]
        public List<string> EventIds { get; set; }

        /// <summary>
        /// Datum
        /// </summary>
        [DataMember(Name="datum")]
        public DateFilter DateFilter { get; set; }

        /// <summary>
        /// Taxon
        /// </summary>
        [DataMember(Name="taxon")]
        public TaxonFilter Taxon { get; set; }

        /// <summary>
        /// Area
        /// </summary>
        [DataMember(Name="area")]
        public GeographicsFilter Area { get; set; }
    }
}
