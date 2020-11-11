using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class ExternalSystemMappingFieldDto
    {
        /// <summary>
        ///     The key in the external system.
        /// </summary>
        public string Key { get; set; }

        public string Description { get; set; }
        public ICollection<ExternalSystemMappingValueDto> Values { get; set; }
    }
}
