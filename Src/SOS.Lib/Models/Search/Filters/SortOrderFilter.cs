using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Filters
{
    public class SortOrderFilter
    {
        /// <summary>
        /// Name of field to sort by
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Sort order, asc or desc
        /// </summary>
        public SearchSortOrder SortOrder { get; set; }
    }
}
