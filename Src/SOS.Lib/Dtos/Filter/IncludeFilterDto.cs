namespace SOS.Lib.Dtos.Filter
{
    public class IncludeFilterDto
    {
        /// <summary>
        /// Include observations with listed occurrence id's
        /// </summary>
        public IEnumerable<string>? OccurrenceIds { get; set; }
    }
}
