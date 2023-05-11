using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
	/// Date filter
	/// </summary>
    public class DateFilter
    {        
        /// <summary>
		/// Event start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
		/// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
		/// Event end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
		/// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
		/// Date filter type
		/// </summary>
        public DateFilterType DateFilterType { get; set; }
    }
}