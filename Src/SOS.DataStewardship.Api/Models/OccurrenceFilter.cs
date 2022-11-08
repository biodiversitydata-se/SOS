using System.Runtime.Serialization;
using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Search filter.
    /// </summary>
    [DataContract]
    public class OccurrenceFilter
    {
        /// <summary>
        /// ExportMode. Todo - this property should not be in a filter?
        /// </summary>
        [DataMember(Name = "exportMode")]
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// DatasetList
        /// </summary>
        [DataMember(Name = "datasetList")]
        public List<string> DatasetList { get; set; }

        /// <summary>
        /// EventIds
        /// </summary>
        [DataMember(Name = "eventIds")]
        public List<string> EventIds { get; set; }

        /// <summary>
        /// Datum
        /// </summary>
        [DataMember(Name="datum")]
        public DatumFilter Datum { get; set; }

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
