using NetTopologySuite.Geometries;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Harvesters
{
    public class AreaHarvestFactory : IHarvestFactory<IEnumerable<AreaEntity>, Area>
    {
        private readonly IDictionary<string, Geometry> _geometries;
        public AreaHarvestFactory(IDictionary<string, Geometry> geometries)
        {
            _geometries = geometries;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Area>?> CastEntitiesToVerbatimsAsync(IEnumerable<AreaEntity> entities)
        {
            return await Task.Run(() =>
            {
                return entities
                    .Select(e => new Area((AreaType) e.AreaDatasetId, e.FeatureId)
                    {
                        Name = e.Name,
                        BoundingBox = LatLonBoundingBox.Create(_geometries[((AreaType)e.AreaDatasetId).ToAreaId(e.FeatureId)].EnvelopeInternal)
                    });
            });
        }
    }
}
