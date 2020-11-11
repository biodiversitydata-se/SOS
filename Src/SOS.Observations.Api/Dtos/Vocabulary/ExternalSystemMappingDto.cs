using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class ExternalSystemMappingDto
    {
        public ExternalSystemIdDto Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ExternalSystemMappingFieldDto> Mappings { get; set; }
    }
}
