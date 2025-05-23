﻿using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
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
        /// Get histogram based on 48 weeks a year
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregated48WeekHistogramAsync(SearchFilter filter);

        /// <summary>
        /// Get histogram based on year
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="timeSeriesType"></param>
        /// <returns></returns>
        Task<IEnumerable<TimeSeriesHistogramResult>> GetYearHistogramAsync(SearchFilter filter, TimeSeriesType timeSeriesType);

        /// <summary>
        /// Get histogram based on a time series
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="timeSeriesType"></param>
        /// <returns></returns>
        Task<IEnumerable<TimeSeriesHistogramResult>> GetTimeSeriesHistogramAsync(SearchFilter filter, TimeSeriesType timeSeriesType);

        /// <summary>
        /// Get geo grid tile aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        Task<GeoGridTileResult> GetGeogridTileAggregationAsync(SearchFilter filter, int zoom);

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
