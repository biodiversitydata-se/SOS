using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Filter
{
    public class DataStewardshipFilterDto
    {
        /// <summary>
        /// Dataset filter
        /// </summary>
        public IEnumerable<string> DatasetIdentifiers { get; set; }
    }
}
