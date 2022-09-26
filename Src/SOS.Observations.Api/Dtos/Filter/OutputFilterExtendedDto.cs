using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Response output settings
    /// </summary>
    public class OutputFilterExtendedDto : OutputFilterDto
    {
        /// <summary>
        /// Sort result
        /// </summary>
        public IEnumerable<SortOrderDto> SortOrders { get; set; }
    }
}
