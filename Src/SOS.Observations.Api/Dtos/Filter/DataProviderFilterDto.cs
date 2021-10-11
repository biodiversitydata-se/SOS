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
        public List<int> Ids { get; set; }
    }
}