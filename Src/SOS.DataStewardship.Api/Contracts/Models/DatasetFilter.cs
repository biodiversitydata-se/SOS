using SOS.Lib.Swagger;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    /// <summary>
	/// Dataset filter
	/// </summary>
    public class DatasetFilter
    {
        /// <summary>
		/// Area filter
		/// </summary>
        public GeographicsFilter Area { get; set; }

        /// <summary>
		/// DatasetIds filter
		/// </summary>
        public List<string> DatasetIds { get; set; }

        [SwaggerExclude]
        public List<string> DatasetList { get; set; }

        /// <summary>
		/// Date filter
		/// </summary>
        public DateFilter DateFilter { get; set; }

        [SwaggerExclude] 
        public DateFilter Datum { get; set; }

        /// <summary>
		/// Taxon filter
		/// </summary>
        public TaxonFilter Taxon { get; set; }
    }
}
