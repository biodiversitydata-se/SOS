using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    public class DataStewardshipFilterDto
    {
        /// <summary>
        /// Dataset filter
        /// </summary>
        public IEnumerable<string>? DatasetIdentifiers { get; set; }
    }
}
