using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;


namespace SOS.Harvest.Harvesters.ClamPortal
{
    public class ClamPortalHarvestFactory : HarvestBaseFactory, IHarvestFactory<IEnumerable<ClamObservationVerbatim>, ClamObservationVerbatim>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<ClamObservationVerbatim>> CastEntitiesToVerbatimsAsync(IEnumerable<ClamObservationVerbatim> entities)
        {
            return await Task.Run(() =>
            {
                return
                    from e in entities
                    select CastEntityToVerbatim(e);
            });
        }

        /// <summary>
        ///     Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ClamObservationVerbatim CastEntityToVerbatim(ClamObservationVerbatim entity)
        {
            entity.Id = NextId;

            return entity;
        }
    }
}
