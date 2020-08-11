using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Harvest
{
    public class AreaHarvestFactory : IHarvestFactory<IEnumerable<AreaEntity>, Area>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<Area>> CastEntitiesToVerbatimsAsync(IEnumerable<AreaEntity> entities, bool incrementalHarvest = false)
        {
            return await Task.Run(() =>
            {
                return from e in entities
                    select new Area((AreaType)e.AreaDatasetId)
                    {
                        Id = e.Id,
                        FeatureId = e.FeatureId,
                        Name = e.Name
                    };
            });
        }
    }
}
