namespace SOS.Analysis.Api.Dtos.Filter
{
    public class ExcludeFilterDto
    {
        /// <summary>
        /// Exclude observations with listed occurrence id's
        /// </summary>
        public IEnumerable<string>? OccurrenceIds { get; set; }
    }
}
