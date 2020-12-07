using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Internal search filter.
    /// </summary>
    public class SearchFilterInternalDto : SearchFilterInternalBaseDto
    {
        /// <summary>
        /// By default totalCount in search response will not exceed 10 000. If IncludeRealCount is true, totalCount will show the real number of hits. Even if it's more than 10 000 (performance cost)  
        /// </summary>
        public bool IncludeRealCount { get; set; }

        /// <summary>
        ///     This parameter allows you to create a dynamic view of the collection, or more precisely,
        ///     to decide what fields should or should not be returned, using a projection. Put another way,
        ///     your projection is a conditional query where you dictates which fields should be returned by the API.
        ///     Omit this parameter and you will receive the complete collection of fields.
        /// </summary>
        public IEnumerable<string> OutputFields { get; set; }
    }
}