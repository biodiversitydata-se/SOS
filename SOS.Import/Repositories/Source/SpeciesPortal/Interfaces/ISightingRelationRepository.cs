using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Entities;
using SOS.Import.Models;
using SOS.Import.Models.Aggregates;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    public interface ISightingRelationRepository
    {
        Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds);
    }
}
