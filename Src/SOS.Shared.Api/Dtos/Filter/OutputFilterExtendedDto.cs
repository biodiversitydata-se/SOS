using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Response output settings
    /// </summary>
    public class OutputFilterExtendedDto : OutputFilterDto
    {
        /// <summary>
        /// Sort result
        /// </summary>
        public IEnumerable<SortOrderDto>? SortOrders { get; set; }
    }
}
