using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Processors.Artportalen.Interfaces
{
    /// <summary>
    ///     Artportalen observation processor
    /// </summary>
    public interface IArtportalenObservationProcessor : IObservationProcessor
    {
        /// <summary>
        /// Process observation verbatims
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="verbatimObservations"></param>
        /// <returns></returns>
        Task<bool> ProcessObservationsAsync(DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IEnumerable<ArtportalenObservationVerbatim> verbatimObservations);
    }
}