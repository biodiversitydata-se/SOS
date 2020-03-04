using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Search filter for advanced search
    /// </summary>
    public class SearchFilter : FilterBase
    {
        /// <summary>
        /// Fields to return (empty = all)
        /// </summary>
        public IEnumerable<string> OutputFields { get; set; }

        public new SearchFilter Clone()
        {
            var advancedFilter = (SearchFilter)MemberwiseClone();
            return advancedFilter;
        }
    }
}
