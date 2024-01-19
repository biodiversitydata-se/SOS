using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Data provider filter.
    /// </summary>
    public class DataProviderFilterDto
    {
        /// <summary>
        ///    Data provider id's
        /// </summary>
        public IEnumerable<int>? Ids { get; set; }
    }
}