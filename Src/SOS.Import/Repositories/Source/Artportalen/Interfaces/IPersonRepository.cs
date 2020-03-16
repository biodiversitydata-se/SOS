using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    public interface IPersonRepository
    {
        Task<IEnumerable<PersonEntity>> GetAsync();
    }
}
