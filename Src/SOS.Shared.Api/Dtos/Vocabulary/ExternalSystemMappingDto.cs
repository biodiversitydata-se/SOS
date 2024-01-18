using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Vocabulary
{
    public class ExternalSystemMappingDto
    {
        public ExternalSystemIdDto Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<ExternalSystemMappingFieldDto> Mappings { get; set; }
    }
}
