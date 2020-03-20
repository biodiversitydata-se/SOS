using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Repository for retrieving processd taxa.
    /// </summary>
    public interface IProcessedFieldMappingRepository : IProcessBaseRepository<FieldMapping, FieldMappingFieldId>
    {
      
    }
}
