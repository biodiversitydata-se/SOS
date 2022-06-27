using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result
{
    public class SearchAfterResult<T>
    {
        /// <summary>
        /// Point in time id
        /// </summary>
        public string PointInTimeId { get; set; }
    
        /// <summary>
        /// Returned records
        /// </summary>
        public IEnumerable<T> Records { get; set; }

        /// <summary>
        /// Search after objects
        /// </summary>
        public IEnumerable<object> SearchAfter { get; set; }
    }
}
