using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    public interface IProcessedObservationRepository : IProcessedObservationCoreRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get geo grid tile aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        Task<GeoGridTileResult> GetGeogridTileAggregationAsync(SearchFilter filter, int zoom);

        /// <summary>
        /// Gets a single observation
        /// </summary>
        /// <param name="occurrenceId"></param>
        /// <returns></returns>
        Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter);

        /// <summary>
        /// Get number of provinces matching the provided filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<int> GetProvinceCountAsync(SearchFilterBase filter);

        /// <summary>
        /// Count the number of user observations group by year
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearCountResult>> GetUserYearCountAsync(SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year and month
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthCountResult>> GetUserYearMonthCountAsync(SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year, month and day
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthDayCountResult>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Check if index have observations with same occurrence id
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex);

        /// <summary>
        /// Signal search
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<bool> SignalSearchInternalAsync(
            SearchFilter filter,
            bool onlyAboveMyClearance);
    }
}
