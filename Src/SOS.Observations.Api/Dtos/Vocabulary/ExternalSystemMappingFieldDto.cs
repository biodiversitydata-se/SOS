using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class ExternalSystemMappingFieldDto
    {
        /// <summary>
        ///     The key in the external system.
        /// </summary>
        public string Key { get; set; }

        public string Description { get; set; }
        public IEnumerable<ExternalSystemMappingValueDto> Values { get; set; }
    }
}
