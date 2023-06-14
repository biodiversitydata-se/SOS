using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Result returned year month aggregation
    /// </summary>
    public class YearMonthDayCountResultDto : YearMonthCountResultDto
    {
        /// <summary>
        ///     Month
        /// </summary>
        public int Day { get; set; }

        public IEnumerable<IdNameDto<string>> Localities { get; set; }
    }
}
