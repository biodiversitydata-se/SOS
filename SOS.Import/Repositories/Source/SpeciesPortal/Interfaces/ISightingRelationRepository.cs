using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Entities;
using SOS.Import.Models;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    public interface ISightingRelationRepository
    {
        Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds);
    }
}
