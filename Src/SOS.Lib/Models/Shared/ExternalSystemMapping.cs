using SOS.Lib.Enums;
using System.Collections.Generic;

namespace SOS.Lib.Models.Shared;

public class ExternalSystemMapping
{
    public ExternalSystemId Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<ExternalSystemMappingField> Mappings { get; set; }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
    }
}