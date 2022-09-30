using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Search filter.
    /// </summary>
    [DataContract]
    public class DatasetFilter
    { 
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
