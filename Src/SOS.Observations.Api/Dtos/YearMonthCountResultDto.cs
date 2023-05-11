namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    ///     Result returned year month aggregation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class YearMonthCountResultDto : YearCountResultDto
    {
        /// <summary>
        ///     Month
        /// </summary>
        public int Month { get; set; }
    }
}
