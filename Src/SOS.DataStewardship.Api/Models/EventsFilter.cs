using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
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
        /// Gets or Sets DatasetList
        /// </summary>        
        [DataMember(Name = "datasetList")]
        public List<string> DatasetList { get; set; }

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
