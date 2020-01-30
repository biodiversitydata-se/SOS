using System.Collections.Generic;
using System.Threading.Tasks;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IFieldMappingVerbatimRepository : IVerbatimBaseRepository<FieldMapping, int>
    {
        Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync();
    }
}
