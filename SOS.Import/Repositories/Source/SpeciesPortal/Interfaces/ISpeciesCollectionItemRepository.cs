using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    public interface ISpeciesCollectionItemRepository
    {
        Task<IEnumerable<SpeciesCollectionItemEntity>> GetAsync();
    }
}
