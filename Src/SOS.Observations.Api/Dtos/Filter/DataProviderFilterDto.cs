using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Data provider filter.
    /// </summary>
    public class DataProviderFilterDto
    {
        /// <summary>
        ///    Data provider id's
        /// </summary>
        public IEnumerable<int> Ids { get; set; }
    }
}