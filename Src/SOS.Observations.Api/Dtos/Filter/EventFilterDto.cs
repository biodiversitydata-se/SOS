using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    public class EventFilterDto
    {
        /// <summary>
        /// Event id's
        /// </summary>
        public IEnumerable<string>? Ids { get; set; }
    }
}
