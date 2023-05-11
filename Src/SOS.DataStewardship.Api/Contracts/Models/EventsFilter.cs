using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.Lib.Swagger;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    /// <summary>
	/// Events search filter
	/// </summary>
    public class EventsFilter
    {        
        /// <summary>
		/// Export mode
		/// </summary>
        public ExportMode ExportMode { get; set; }
        
        /// <summary>
		/// DatasetIds filter
		/// </summary>
        public List<string> DatasetIds { get; set; }

        [SwaggerExclude]
        public List<string> DatasetList { get; set; }

        /// <summary>
		/// EventIds filter
		/// </summary>
        public List<string> EventIds { get; set; }

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

        /// <summary>
		/// Area filter
		/// </summary>
        public GeographicsFilter Area { get; set; }
    }
}
