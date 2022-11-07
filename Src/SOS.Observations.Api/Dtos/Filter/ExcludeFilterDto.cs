using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    public class ExcludeFilterDto
    {
        /// <summary>
        /// Exclude observations with listed occurrence id's
        /// </summary>
        public IEnumerable<string>? OccurrenceIds { get; set; }
    }
}
