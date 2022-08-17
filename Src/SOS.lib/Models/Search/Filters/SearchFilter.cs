using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchFilter : SearchFilterBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sensitiveObservations"></param>
        public SearchFilter(int userId, bool sensitiveObservations = false) : base(userId, sensitiveObservations)
        {

        }

        /// <summary>
        ///     This parameter allows you to create a dynamic view of the collection, or more precisely,
        ///     to decide what fields should or should not be returned, using a projection. Put another way,
        ///     your projection is a conditional query where you dictates which fields should be returned by the API.
        ///     Omit this parameter and you will receive the complete collection of fields.
        /// </summary>
        public List<string> OutputFields { get; set; }

        public new SearchFilter Clone()
        {
            var searchFilter = (SearchFilter)MemberwiseClone();
            return searchFilter;
        }
    }
}