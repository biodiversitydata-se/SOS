using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result
{
    /// <summary>
    ///     Result returned year month aggregation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class YearMonthDayCountResult : YearMonthCountResult
    {
        /// <summary>
        ///     Day
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Localities
        /// </summary>
        public ICollection<IdName<string>> Localities { get; set; }
    }
}