using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface IPersonRepository
    {
        Task<IEnumerable<PersonEntity>> GetAsync();
    }
}