using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Interfaces
{
    public interface IFieldMappingValue
    {
        int Id { get; set; }
        string Description { get; set; }
        ICollection<FieldMappingTranslation> Translations { get; set; }
    }
}
