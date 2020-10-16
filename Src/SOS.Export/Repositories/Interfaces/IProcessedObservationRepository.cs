using System.Threading.Tasks;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<Observation, string>
    {
        /// <summary>
        ///     Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<Project>> ScrollProjectParametersAsync(FilterBase filter, string scrollId);

        /// <summary>
        ///     Get observation by scroll
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<Observation>> ScrollObservationsAsync(FilterBase filter, string scrollId);

        /// <summary>
        ///     Get observation by scroll. 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <remarks>To improve performance this method doesn't use the dynamic type.</remarks>
        /// <returns></returns>
        Task<ScrollResult<Observation>> TypedScrollObservationsAsync(
            FilterBase filter,
            string scrollId);

        /// <summary>
        ///     Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <remarks>To improve performance this method doesn't use the dynamic type.</remarks>
        /// <returns></returns>
        Task<ScrollResult<ExtendedMeasurementOrFactRow>> TypedScrollProjectParametersAsync(
            FilterBase filter,
            string scrollId);
    }
}